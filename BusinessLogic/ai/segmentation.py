# import some common libraries
import base64
import cv2
import json
import sys
import numpy as np
import ast
import torch

# import some common detectron2 utilities
from detectron2.config import get_cfg
from detectron2.data.catalog import Metadata
from detectron2.engine import DefaultPredictor

import pytesseract

# import some common libraries
import base64
import ast
import json
import cv2
import sys
import numpy as np
from PIL import Image
from paddleocr import PaddleOCR


# class to store book information
class Book:
    def __init__(self, x1, x2, x3, x4, y1, y2, y3, y4, recognizedText):
        self.x1 = x1
        self.x2 = x2
        self.x3 = x3
        self.x4 = x4
        self.y1 = y1
        self.y2 = y2
        self.y3 = y3
        self.y4 = y4
        self.RecognizedText = recognizedText


def load_model(path_to_config, path_to_model):
    thresh = 0.5
    # create a configuration object
    cfg = get_cfg()
    cfg.merge_from_file(path_to_config)
    cfg.MODEL.ROI_HEADS.SCORE_THRESH_TEST = thresh
    if not torch.cuda.is_available():
        cfg.MODEL.DEVICE = 'cpu'

    # load weights
    cfg.MODEL.WEIGHTS = path_to_model

    # create predictor(model) object
    predictor = DefaultPredictor(cfg)

    return predictor


def load_image_base64(image_data):
    # decode the base64 string to binary data
    binary_data = base64.b64decode(image_data)

    # convert the binary data to a numpy array
    buf = np.frombuffer(binary_data, dtype=np.uint8)

    # decode the numpy array into an OpenCV image object
    img = cv2.imdecode(buf, cv2.IMREAD_COLOR)

    return img


# image preprocess before ocr
def preprocess(image):
    # upscale image
    if image.shape[0] < 100 or image.shape[1] < 100:
        scale_percent = 200  # percent of original size
        width = int(image.shape[1] * scale_percent / 100)
        height = int(image.shape[0] * scale_percent / 100)
        dim = (width, height)
        image = cv2.resize(image, dim, interpolation=cv2.INTER_LINEAR)
    # sharpen image
    sharp = cv2.filter2D(image, -1, kernel=np.array([[-1, -1, -1], [-1, 9, -1], [-1, -1, -1]]))
    # sharp = cv2.medianBlur(sharp, 1)
    sharp = cv2.bilateralFilter(sharp, 9, 75, 75)

    return sharp


def ocr(image, ocr_paddle):
    # config for tesseract
    cfg = "--psm 10"
    lng = 'rus'

    # get text boxes from bookspine
    boxes = ocr_paddle.ocr(image, det=True, rec=False)
    # store words of while segmented image
    words = []
    for bbox in boxes[0]:
        if len(bbox) == 0:
            pass
        # get coordinates
        x_min, x_max = min([int(x[0]) for x in bbox]), max([int(x[0]) for x in bbox])
        y_min, y_max = min([int(y[1]) for y in bbox]), max([int(y[1]) for y in bbox])
        h = y_max - y_min
        w = x_max - x_min
        # crop text area
        cropped_img = image[y_min:y_min + h, x_min:x_min + w]
        (h, w) = cropped_img.shape[:2]
        # store found words on text image
        word_lst = []
        # in case of vertical spine
        if h > w:
            # Rotate the image by 90 image clockwise
            rotated = cv2.rotate(cropped_img, cv2.ROTATE_90_CLOCKWISE)
            ocr_clockwise = pytesseract.image_to_data(rotated, lang=lng, config=cfg, output_type='data.frame')
            ocr_clockwise = ocr_clockwise[(ocr_clockwise.conf != -1)]

            # Rotate the image by 90 degrees clockwise
            rotated = cv2.rotate(cropped_img, cv2.ROTATE_90_COUNTERCLOCKWISE)
            ocr_counter = pytesseract.image_to_data(rotated, lang=lng, config=cfg, output_type='data.frame')
            ocr_counter = ocr_counter[ocr_counter.conf != -1]

            # choose result in both, clockwise and counterclockwise case
            if (not ocr_counter.empty) and (not ocr_clockwise.empty):
                max_clockwise = ocr_clockwise['conf'].max()
                max_counter = ocr_counter['conf'].max()
                if max_clockwise < max_counter:
                    word_lst = ocr_counter['text'].tolist()
                else:
                    word_lst = ocr_clockwise['text'].tolist()
            elif not ocr_counter.empty:
                word_lst = ocr_counter['text'].tolist()
            elif not ocr_clockwise.empty:
                word_lst = ocr_clockwise['text'].tolist()
        # horizontal spine
        else:
            ocr_st = pytesseract.image_to_data(cropped_img, lang=lng, config=cfg, output_type='data.frame')
            ocr_st = ocr_st[(ocr_st.conf != -1)]
            word_lst = ocr_st['text'].tolist()

        # post process text
        word_lst = [str(s).lower() for s in word_lst if len(str(s)) > 2]
        words.extend(word_lst)
    return words


# load files for model
path_to_config = sys.argv[1]
path_to_model = sys.argv[2]
# load model
predictor = load_model(path_to_config, path_to_model)

# load OCR model for russian language
ocr_paddle = PaddleOCR(use_angle_cls=True, lang="ru")

# load image by base64 data
base64_path = sys.argv[3]
with open(base64_path, 'r') as f:
    image_data = f.read()
im = load_image_base64(image_data)

# make prediction on segmentation
outputs = predictor(im)

"""from detectron2.utils.visualizer import Visualizer
from detectron2.utils.visualizer import ColorMode
from detectron2.data.catalog import Metadata

my_metadata = Metadata()
my_metadata.set(thing_classes = ['books', 'book spine'])
v = Visualizer(im[:, :, ::-1],
    metadata=my_metadata,
    scale=0.5,
    instance_mode=ColorMode.IMAGE_BW   # remove the colors of unsegmented pixels. This option is only available for segmentation models
  )
out = v.draw_instance_predictions(outputs["instances"].to("cpu"))
cv2.imshow("window_name",out.get_image()[:, :, ::-1])
cv2.waitKey(0)

# closing all open windows
cv2.destroyAllWindows()"""


# get masks of found objects
masks = np.asarray(outputs["instances"].pred_masks.to("cpu"))

# iterate every found obj(book)
books = []
for item_mask in masks:
    # Get the true bounding box of the mask
    segmentation = np.where(item_mask == True)

    # Get coordinates of bounding box
    x_min = int(np.min(segmentation[1]))
    x_max = int(np.max(segmentation[1]))
    y_min = int(np.min(segmentation[0]))
    y_max = int(np.max(segmentation[0]))

    # crop book spine from image
    cropped = Image.fromarray(im[y_min:y_max, x_min:x_max, :], mode='RGB')
    cropped = np.array(cropped)
    # preprocess image
    cropped = preprocess(cropped)

    # get text
    text = ocr(cropped, ocr_paddle)
    print(text)
    # store it in object
    book = Book(x_min, x_max, x_max, x_min, y_max, y_max, y_min, y_min, text)

    books.append(book.__dict__)

# Save the JSON-formatted string to a file
result = json.dumps(books)
# print(result)
with open('books.json', 'w') as f:
    f.write(result)
