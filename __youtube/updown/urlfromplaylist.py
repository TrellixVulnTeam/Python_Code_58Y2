from bs4 import BeautifulSoup
import requests
minh = open('result.txt', 'a')
class Playlist():
    def __init__(self, playListUrl):
        self._playListUrl = playListUrl

        # This will take the html text from Youtube playList url and stores it in a variable called html-doc.
        self._htmldoc = requests.get(str(self._playListUrl)).text
        self._soup = BeautifulSoup(self._htmldoc, 'html.parser')

        # This will create a list of all the titles and the youtube url videos using the html-doc.
        self._rawList = self._soup('a', {'class': 'pl-video-title-link'})

        # This will loop through a list of titles and Youtube urls and formats it nicely for you.
        for link in self._rawList:
            # print('{0}'.format(link.string) + 'http://youtube.com' + '{0}'.format(link.get('href')))
            # print('http://youtube.com' + '{0}'.format(link.get('href')[:20]))
            minh.write('http://youtube.com' + '{0}'.format(link.get('href')[:20]))
            minh.write('\n')

# To use this class all you got to do is:
# 1 - Create a new object to use the class..
# 2- put a youtube playlist url where it is shown below..
# 3- Run it, and enjoy.
objPlaylist = Playlist('https://www.youtube.com/playlist?list=PLzVm1SmjPKc_OnC56MbILOmdXvGV_3kE9')