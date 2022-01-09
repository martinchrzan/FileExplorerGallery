# FileExplorerGallery
## Quick gallery integrated into Windows File Explorer

*FileExplorerGallery* will let you quickly view all images in the currently opened folder. 

Based on the image gallery in [Atlasee](https://store.steampowered.com/app/1052330/Atlasee/)

# [**Download the latest release here**](https://github.com/martinchrzan/FileExplorerGallery/releases/latest)

## **Open**
Simply press F12 (default shortcut, can be changed) to open it up, when you are in the desired folder

![Open](Images/FileExplorerGalleryOpen.gif)

Once the *FileExplorerGallery* is opened, you can navigate between your images using keyboard or mouse.

## **Zoom**
You can zoom in by double-clicking or using a mouse scroll. To reset back, press a right mouse button.

![Zoom](Images/FileExplorerGalleryZoomUnzoom.gif)

## **Rotate**
You can rotate your image by pressing the rotate icon on the top border

![Rotate](Images/FileExplorerGalleryRotate.gif)

## **Slideshow**
To run a slideshow of your images (you can configure the duration in the settings), press the slideshow button on the top border.

![Slideshow](Images/FileExplorerGallerySlideshow.gif)


## **Other actions**
- To delete currently displayed image, press "Delete" button or trash can icon in the top bar
  - By default, you will get a confirmation message, if you don't want to display a confirmation message on delete, you can disable it in the settings - "Show confirmation on delete"
  - If you want to move your images into "Backup" folder instead of deleting them directly, you can enable setting "Backup deleted images" which will move deleted images into the specified folder instead.
  - ![image](https://user-images.githubusercontent.com/11967522/148691740-b5952930-5199-4343-bdec-bb0cadf5208a.png)


- To save image after you rotated it, press "Ctrl+S" or save icon in the top bar

- Right clicking the application tray icon you can open its settings.


- *FileExplorerGallery* supports auto-update and can start on the Windows startup. 

### Windows File Explorer integration points:
- Shows images in the currently focused window - *FileExplorerGallery* will be opened on the same monitor if you have multiple.
- Selected image in the File Explorer will be selected in *FileExplorerGallery* when it shows up
- *FileExplorerGallery* follows the same ordering as you currently have in your folder (please note, if you make a change in your folder ordering, you need to go out of this folder and come back in order for *FileExplorerGallery* to pick it up. This is the current limitation)

**NOTE**: After starting *FileExplorerGallery*, it might take up to 10 seconds before you can use it (Windows API to get opened File Explorer Windows needs to "warm up") 
