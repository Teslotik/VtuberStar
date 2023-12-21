'''
Обрезает пустую область на изображении (с нулевой Alpha)
'''
from PIL import Image, ImageColor, ImageDraw
import sys


for pathname in sys.argv[1:]:
    with Image.open(pathname) as image:
        w, h = image.size
        minx = None
        miny = None
        maxx = None
        maxy = None
        for x in range(w):
            for y in range(h):
                r, g, b, a = image.getpixel((x, y))
                if a == 0: continue
                if minx is None or x < minx: minx = x
                if miny is None or y < miny: miny = y
                if maxx is None or x > maxx: maxx = x
                if maxy is None or y > maxy: maxy = y
        print(f'{pathname}', minx, miny, maxx, maxy)
        modified = image.crop((minx, miny, maxx, maxy))
    modified.save(pathname)
