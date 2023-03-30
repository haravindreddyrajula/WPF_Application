# WPF_Application

Coding Assignment
----------

	A WPF application that allows the user to perform the following tasks:
		1. Select and load an image.
		2. Draw one rectangle or more over the image by clicking and dragging the mouse to draw (the size of the rectangle depends on how much the user drags the mouse)
		3. Only allow drawing inside the picture.
		4. Change the rectangle's color by clicking each rectangle and selecting a different color from a color palette.
		5. Resize the rectangle(s) from any corner or side.
		6. Move the rectangle by pressing and holding the rectangle and moving the mouse (drag and drop).
		7. Delete any rectangle.
		8. Save the changes to a new image.

Stack
-----
```
-.NETFramework,Version=v4.7.2
Additionally used "System.Text.Json" package
```

Usage
-----

	Two ways to launch an application
		1. Standalone application  
			Going to the `./publish` folder and clicking the `setup.exe` folder.
		2. By using the `WpfApp2.exe` located `./bin/Debug`
	
Instructions on Using the Features
----------------------------------

- Load an Image
    ```
    Click the "Select Image" button to select and load an image from your local PC. It will load the image together with the "Feedback" and "Help" buttons and the "Select Image" will be collapsed.
	```
- Drawing Rectangle(s) on the Image
	```
    Click and drag the mouse on the image to draw a rectangle. The size of the rectangle depends on how much the user drags the mouse. But dragging is only allowed inside the image boundary.
	```
- Selecting Rectangle(s)
	```
	Click the rectangle(s) to select it. Once you click atleast one rectangle the rectangle border changes to "red" color. 
	The "Color" and "Delete" buttons get visible. 
	```
- Selecting Rectangle(s)
	```
	Click on the selected rectangle(s) to deselect. The rectangle(s) border color changes to "black".
	Once there are no selected rectangles (No rectangle border is "red"). The "Color" and "Delete" buttons get collapse. 
	```
- Change the Rectangle(s) Color
	```
    Select the rectangle(s)
    Click the "color" button and then select a color from the color palette to change the rectangle's color.
	If user didnt select, default color "black" is applied.
	```
- Delete the Rectangle(s)
    ```
    Select the rectangle(s)
    Click the "Delete" button.
	This feature has been enhanced to "Delete multiple rectangles all at once".
	```
- Resize the Rectangle(s)
	```
    Hover over any corner or side of the rectangle, but not inside the rectangle shape.
    Click and drag the mouse to resize the rectangle. The resizing is done only within the image bounds.
	```
- Move the Rectangle(s)
	```
    Click and hold a rectangle to select it. but not on the borders of the rectangle.
    Drag the mouse to move the selected rectangle. user can drag only inside the image bounds.
    Release the mouse to drop the rectangle at the new location.
	```
- Save the Changes to a New Image
    ```
    Click the "Save Image" button to save the changes, user can select the location to save in the dialog box.
    The new image will be saved to the specified location together with the metadata stored in json file with the same iamge name.
	```
- Additional Point 
	```
	A user can reload the image and can continue to work till he developed by clicking "select Image" button.
	But the rectangle color is not stored in the metadata. This feature is yet to be completed.
	```
- Additional Features

	1. Help button
		If user clicks "help" button. The readme file is displayed on separate dailog box
	2. Feeback button
		If user clicks "feedback button". User can enter his feedback on the opened dialog box.
	3. Scroll option
		To make user comfortable a "scroller" is introduced at the right.
		

Developer details
------------------
```
Haravind Reddy Rajula 
Graduate Research Assistant
Center for Real-Time Distributed Sensing and Autonomy Lab, UMBC
Computer Science Graduate | UMBC | Baltimore, MD
Ph: 667.900.2815 | E: haravindreddyrajula@gmail.com / hrajula1@umbc.edu
```