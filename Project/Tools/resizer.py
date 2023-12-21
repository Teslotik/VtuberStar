'''
Изменяет масштаб изображений
'''

from PIL import Image, ImageColor, ImageDraw
import sys

for pathname in sys.argv[1:]:
    with Image.open(pathname) as image:
        image.resize((64, 64)).save(pathname)