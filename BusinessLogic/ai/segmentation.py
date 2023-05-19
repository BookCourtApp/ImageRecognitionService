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


# class to store book information
class Book:
    def __init__(self, height, width, x, y, recognizedText):
        self.Height = height
        self.Width = width
        self.x = x
        self.y = y
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


# load files for model
path_to_config = sys.argv[1]
path_to_model = sys.argv[2]
# load model
predictor = load_model(path_to_config, path_to_model)

# load image by base64 data
base64_path = sys.argv[3]
with open(base64_path, 'r') as f:
    image_data = f.read()
im = load_image_base64(image_data)

# make prediction
outputs = predictor(im)

# get masks of found objects
masks = np.asarray(outputs["instances"].pred_masks.to("cpu"))

# iterate every found obj(book)
book_coord = []
for item_mask in masks:
    # Get the true bounding box of the mask
    segmentation = np.where(item_mask == True)

    x_min = int(np.min(segmentation[1]))
    x_max = int(np.max(segmentation[1]))
    y_min = int(np.min(segmentation[0]))
    y_max = int(np.max(segmentation[0]))

    # store it in object
    book = Book(y_max-y_min, x_max-x_min, x_min, y_min, [])

    book_coord.append(book.__dict__)
# save to json
result = json.dumps(book_coord)
#print(result)

# Save the JSON-formatted string to a file
with open('books.json', 'w') as f:
    f.write(result)
