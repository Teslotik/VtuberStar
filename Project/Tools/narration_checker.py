'''
Используется для сравнения файла с повествованием (first) с исходным кодом находящимся в directory
Ненайденные строки будут выведены в консоль
'''

# TODO Rename
first = r'H:\Projects\TimeRomantic\backup\text.txt'
directory = r'H:\Projects\TimeRomantic\Time Romantic'

import os, sys

with open(first, 'r', encoding = 'utf-8') as file:
    source = file.read().splitlines()
    source = [l.replace('"', '') for l in source]
    source = [l for l in source if not l.startswith('//')]

    for root, dirs, files in os.walk(directory):
        for name in files:
            if not name.endswith('.cs'): continue
            with open(os.path.join(root, name), 'r', encoding = 'utf-8') as file:
                destination = file.read()

                source = [l for l in source if l not in destination]
    
    for line in source:
        print(line)