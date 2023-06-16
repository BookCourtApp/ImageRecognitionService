# import some common libraries
import base64
import cv2
import json
import sys
import numpy as np
import ast
import torch
import matplotlib.pyplot as plt
from shapely.geometry import Polygon
import re

# import some common detectron2 utilities
from detectron2.config import get_cfg
from detectron2.data.catalog import Metadata
from detectron2.engine import DefaultPredictor
from detectron2.structures import BoxMode
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
    sharp = cv2.filter2D(image, -1,
                         kernel=np.array(
                             [[-1, -1, -1], [-1, 9, -1], [-1, -1, -1]]
                         ))
    # sharp = cv2.medianBlur(sharp, 1)
    sharp = cv2.bilateralFilter(sharp, 9, 75, 75)

    return sharp


def remake_word(word):
    if isinstance(word, float):
        return str(int(word)).lower()

    filtered = remove_non_alphanumeric(word)
    return filtered.lower()


green_words = ['ар', 'ан', 'аз', 'ах', 'аж', 'ау', 'ад', 'ас', 'аи', 'ао',
               'бы', '', '',
               'во',
               'го',
               'до', 'де',  'да',
               'ее', 'её', 'еж', 'ёж', 'ея',
               'же',
               'оз', 'об', 'ош', 'от',
               'за',
               'ил',  'из', 'ия', 'иа',
               'ко',
               'ль',
               'мы', 'ми',
               'по',
               'ни', 'на', 'ну', 'но',
               'он', 'ом', 'об', 'ох', 'ор',
               'ре',
               'ля',
               'по', 'па',
               'со', 'се', 'си',
               'ум', 'уж', 'ус',
               'ша',
               'фа',
               'щи',
               'эй',
               'юс', 'юг', 'юз', 'юк',
               'яд', 'яр', 'ям', 'ял', 'яо', 'ян', 'як', 'яр'
               ]


def filter_words(word):
    if len(word) < 3:
        if word in green_words:
            return True
        return False
    if word.count(word[0]) == len(word):
        return False
    return True


def remove_non_alphanumeric(input_string):
    pattern = re.compile('[\W_]+')
    return pattern.sub('', input_string)


def ocr(image, ocr_paddle):
    # config for tesseract
    cfg = "--psm 10"
    lng = 'rus'

    if image.shape[0] < 100 or image.shape[1] < 100:
        scale_percent = 200  # percent of original size
        width = int(image.shape[1] * scale_percent / 100)
        height = int(image.shape[0] * scale_percent / 100)
        dim = (width, height)
        image = cv2.resize(image.copy(), dim, interpolation=cv2.INTER_LINEAR)

    # get text boxes from bookspine
    boxes = ocr_paddle.ocr(image, det=True, rec=False)
    # store words of while segmented image
    words = []
    tmp_word_cond = []
    for bbox in boxes[0]:
        if len(bbox) == 0:
            pass
        # get coordinates
        x_min, x_max = min([int(x[0]) for x in bbox]), \
                       max([int(x[0]) for x in bbox])
        y_min, y_max = min([int(y[1]) for y in bbox]), \
                       max([int(y[1]) for y in bbox])
        h = y_max - y_min
        w = x_max - x_min
        # crop text area
        cropped_img = image[y_min:y_min + h, x_min:x_min + w]
        (h, w) = cropped_img.shape[:2]
        # store found words on text image
        word_lst = []
        # in case of vertical spine
        if h > w:
            counter_use, clockwise_use = False, False
            # Rotate the image by 90 image clockwise
            rotated = cv2.rotate(cropped_img, cv2.ROTATE_90_CLOCKWISE)
            ocr_clockwise = pytesseract.image_to_data(rotated, lang=lng, config=cfg, output_type='data.frame')
            ocr_clockwise = ocr_clockwise[(ocr_clockwise.conf > 0)]

            # Rotate the image by 90 degrees clockwise
            rotated = cv2.rotate(cropped_img, cv2.ROTATE_90_COUNTERCLOCKWISE)
            ocr_counter = pytesseract.image_to_data(rotated, lang=lng, config=cfg, output_type='data.frame')
            ocr_counter = ocr_counter[ocr_counter.conf > 0]

            # choose result in both, clockwise and counterclockwise case
            if (not ocr_counter.empty) and (not ocr_clockwise.empty):
                max_clockwise = ocr_clockwise['conf'].max()
                max_counter = ocr_counter['conf'].max()
                if max_clockwise < max_counter:
                    counter_use = True
                else:
                    clockwise_use = True
            elif not ocr_counter.empty:
                counter_use = True
            elif not ocr_clockwise.empty:
                clockwise_use = True

            if clockwise_use:
                tmp_word_cond.append(ocr_clockwise[['text', 'conf']].values.tolist())
                word_lst = ocr_clockwise['text'].tolist()
            if counter_use:
                tmp_word_cond.append(ocr_counter[['text', 'conf']].values.tolist())
                word_lst = ocr_counter['text'].tolist()

        # horizontal spine
        else:
            ocr_st = pytesseract.image_to_data(cropped_img, lang=lng, config=cfg, output_type='data.frame')
            ocr_st = ocr_st[(ocr_st.conf > 0)]

            tmp_word_cond.append(ocr_st[['text', 'conf']].values.tolist())
            word_lst = ocr_st['text'].tolist()

        # post process text
        word_lst = [remake_word(s) for s in word_lst]
        word_lst = [s for s in word_lst if filter_words(s)]
        words.extend(word_lst)
    for cell in tmp_word_cond:
        for text in cell:
            t = remake_word(text[0])
            if not filter_words(t) or text[1] < 1:
                print(f"dropped {text[0]}, conf {text[1]}")
            else:
                print(f"pass    {text[0]}, conf {text[1]}")

    return words


def plt_images(im, segm_coord):
    # Plot each image using imshow()
    num_rows = int(np.ceil(np.sqrt(len(segm_coord))))
    fig, axs = plt.subplots(int(np.ceil(len(segm_coord) / 4)) + 1, 4)
    i = 0
    for item_mask, ax in zip(segm_coord, axs.flat):
        # Get coordinates of bounding box
        x_min = item_mask[0]
        x_max = item_mask[1]
        y_min = item_mask[2]
        y_max = item_mask[3]

        # crop book spine from image
        cropped = Image.fromarray(im[y_min:y_max, x_min:x_max, :], mode='RGB')
        cropped = np.array(cropped)
        # preprocess image
        cropped = preprocess(cropped)
        ax.imshow(cropped, cmap='gray')
        ax.axis('off')
    # Show the plot
    plt.show()


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
# get masks of found objects
masks = np.asarray(outputs["instances"].pred_masks.to("cpu"))

# iterate every found obj(book)
books = []
first = True
segm_coord = []
for item_mask in masks:
    # Get the true bounding box of the mask
    segmentation = np.where(item_mask == True)

    # Get coordinates of bounding box
    x_min = int(np.min(segmentation[1]))
    x_max = int(np.max(segmentation[1]))
    y_min = int(np.min(segmentation[0]))
    y_max = int(np.max(segmentation[0]))

    segm_coord.append([x_min, x_max, y_min, y_max])

for item_mask in segm_coord:
    # Get coordinates of bounding box
    x_min = item_mask[0]
    x_max = item_mask[1]
    y_min = item_mask[2]
    y_max = item_mask[3]

    # crop book spine from image
    cropped = Image.fromarray(im[y_min:y_max, x_min:x_max, :], mode='RGB')
    cropped = np.array(cropped)
    # preprocess image
    cropped = preprocess(cropped)
    # get text
    text = ocr(cropped, ocr_paddle)
    # print(text)
    # store it in object
    book = Book(x_min, x_max, x_max, x_min, y_max, y_max, y_min, y_min, text)
    books.append(book.__dict__)

idx_to_del = []
for i in range(len(books)):
    for j in range(len(books)):
        if j == i:
            continue
        book1 = books[i]
        book2 = books[j]
        poly1 = Polygon([(book1['x1'], book1['y1']),
                         (book1['x2'], book1['x2']),
                         (book1['x3'], book1['x3']),
                         (book1['x4'], book1['x4'])])
        poly2 = Polygon([(book2['x1'], book2['y1']),
                         (book2['x2'], book2['y2']),
                         (book2['x3'], book2['y3']),
                         (book2['x4'], book2['y4'])])

        # Calculate the intersection area between poly1 and poly2
        intersection = poly1.intersection(poly2)
        intersection_area = intersection.area

        # Calculate the area of the smaller polygon
        poly1_area = poly1.area
        poly2_area = poly2.area
        smaller_area = min(poly1_area, poly2_area)

        smaller2 = True
        if poly2_area > poly1_area:
            smaller2 = False

        # Calculate the percentage of the smaller polygon that is inside the larger polygon
        percentage_inside = intersection_area / smaller_area * 100
        # print(percentage_inside)
        if percentage_inside > 90:
            # idx_to_del.append(m)
            if smaller2:
                idx_to_del.append(j)
                books[i]['RecognizedText'].extend(books[j]['RecognizedText'])

                books[i]['x1'] = min(book1['x1'], book2['x1'])
                books[i]['y1'] = max(book1['y1'], book2['y1'])

                books[i]['x2'] = max(book1['x2'], book2['x2'])
                books[i]['y2'] = max(book1['y2'], book2['y2'])

                books[i]['x3'] = max(book1['x3'], book2['x3'])
                books[i]['y3'] = min(book1['y3'], book2['y3'])

                books[i]['x4'] = min(book1['x4'], book2['x4'])
                books[i]['y4'] = min(book1['y4'], book2['y4'])
            else:
                idx_to_del.append(i)
                books[j]['RecognizedText'].extend(books[i]['RecognizedText'])

                books[j]['x1'] = min(book1['x1'], book2['x1'])
                books[j]['y1'] = max(book1['y1'], book2['y1'])

                books[j]['x2'] = max(book1['x2'], book2['x2'])
                books[j]['y2'] = max(book1['y2'], book2['y2'])

                books[j]['x3'] = max(book1['x3'], book2['x3'])
                books[j]['y3'] = min(book1['y3'], book2['y3'])

                books[j]['x4'] = min(book1['x4'], book2['x4'])
                books[j]['y4'] = min(book1['y4'], book2['y4'])

books = [x for i, x in enumerate(books) if i not in idx_to_del]
"""
for i, book in enumerate(books):
        print(book['RecognizedText'])
        x_min = book['x1']
        x_max = book['x3']
        y_min = book['y4']
        y_max = book['y1']

        # crop book spine from image
        cropped = Image.fromarray(im[y_min:y_max, x_min:x_max, :], mode='RGB')
        cropped = np.array(cropped)
        cv2.imshow("window_name", cropped)
        cv2.waitKey(0)
        cv2.destroyAllWindows()
"""
for book in books:
    print(book)
# Save the JSON-formatted string to a file
result = json.dumps(books)
# print(result)
with open('books.json', 'w') as f:
    f.write(result)
