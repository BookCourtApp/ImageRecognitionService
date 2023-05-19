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
    def __init__(self, height, width, x, y, recognizedText):
        self.Height = height
        self.Width = width
        self.x = x
        self.y = y
        self.RecognizedText = recognizedText


# image preprocess before ocr
def preprocess(image):
    #upscale image
    if image.shape[0] < 100 or image.shape[1] < 100:
        scale_percent = 200  # percent of original size
        width = int(image.shape[1] * scale_percent / 100)
        height = int(image.shape[0] * scale_percent / 100)
        dim = (width, height)
        image = cv2.resize(image, dim, interpolation=cv2.INTER_LINEAR)
    # sharpen image
    sharp = cv2.filter2D(image, -1, kernel=np.array([[-1, -1, -1], [-1, 9, -1], [-1, -1, -1]]))
    sharp = cv2.medianBlur(sharp, 1)
    sharp = cv2.bilateralFilter(sharp, 9, 75, 75)

    return sharp


def load_image_base64(image_data):
    # decode the base64 string to binary data
    binary_data = base64.b64decode(image_data)

    # convert the binary data to a numpy array
    buf = np.frombuffer(binary_data, dtype=np.uint8)

    # decode the numpy array into an OpenCV image object
    img = cv2.imdecode(buf, cv2.IMREAD_COLOR)

    return img


# load image by base64 data
base64_path = sys.argv[1]
with open(base64_path, 'r') as f:
    image_data = f.read()
im = load_image_base64(image_data)

# load OCR model for russian language
ocr = PaddleOCR(use_angle_cls=True, lang="ru")

# load results from segmentation module
with open('books.json', 'r') as f:
    books = json.load(f)

books_obj = []
for book in books:
    print(book)
    # get markup
    height = int(book['Height'])
    width = int(book['Width'])
    x = int(book['x'])
    y = int(book['y'])
    # crop
    cropped = Image.fromarray(im[y:y + height, x:x + width, :], mode='RGB')
    cropped = np.array(cropped)
    # preprocess
    cropped = preprocess(cropped)
    # get text bag from image
    res_paddle = ocr.ocr(np.array(cropped))
    # filter one symbols
    wordlist = [word[1][0] for word in res_paddle[0] if len(word[0][1]) > 1]
    # store as object
    book_obj = Book(book['Height'], book['Width'], book['x'], book['y'], wordlist)

    books_obj.append(book_obj.__dict__)

result = json.dumps(books_obj)
