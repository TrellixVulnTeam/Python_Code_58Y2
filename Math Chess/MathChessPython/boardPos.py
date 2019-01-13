grid = [[ [9,'X'], [8, 'X'], [7, 'X'], [6, 'X'], [5, 'X'], [4, 'X'], [3, 'X'], [2, 'X'], [1, 'X']], \
        [ [None,None], [None,None], [None,None], [None,None], [0, 'X'], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None], [None,None]], \
        [ [None,None], [None,None], [None,None], [None,None], [0, 'Y'], [None,None], [None,None], [None,None], [None,None]], \
        [ [1, 'Y'], [2, 'Y'], [3, 'Y'], [4, 'Y'], [5, 'Y'], [6, 'Y'], [7, 'Y'], [8, 'Y'], [9, 'Y']], ]

def boardPos (mouseX, mouseY):

    if (mouseY < 60):
        row = 0
    elif (mouseY < 120):
        row = 1
    elif (mouseY < 180):
        row = 2
    elif (mouseY < 240):
        row = 3
    elif (mouseY < 300):
        row = 4
    elif (mouseY < 360):
        row = 5
    elif (mouseY < 420):
        row = 6
    elif (mouseY < 480):
        row = 7
    elif (mouseY < 540):
        row = 8
    elif (mouseY < 600):
        row = 9
    else:
        row = 10


    if (mouseX < 60):
        col = 0
    elif (mouseX < 120):
        col = 1
    elif (mouseX < 180):
        col = 2
    elif (mouseX < 240):
        col = 3
    elif (mouseX < 300):
        col = 4
    elif (mouseX < 360):
        col = 5
    elif (mouseX < 420):
        col = 6
    elif (mouseX < 480):
        col = 7
    else:
        col = 8

    return (row, col)