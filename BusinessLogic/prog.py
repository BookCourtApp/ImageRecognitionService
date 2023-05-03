import sys
import time
import json 

if len(sys.argv) != 2:
    print("Usage: python program.py arg1")
    sys.exit(1)

arg1 = sys.argv[1]
photos = json.loads(arg1)

for photo in photos:
    print(photo["Image"])
    for book in photo["BookMarkups"]:
        print(book["Width"])
        print(book["Height"])
        print(book["x"])
        print(book["y"])
        for text in book["TextMarkups"]:
            print(text["Type"])
            print(text["Text"])
            print(text["Width"])
            print(text["Height"])
            print(text["x"])
            print(text["y"])
#Или так 
photos["BookMarkups"][0]["TextMarkups"][0]["Text"] # чтобы добраться до размеченного текста, допустим

result = "end"
print(result)