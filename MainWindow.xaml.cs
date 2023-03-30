using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Ink;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using System.Xml.Linq;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class Data
    {
        public Uri imageURI { get; set; }
        public List<Rect> rect { get; set; }
    }

    public class Rect
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
       
    }

    public partial class MainWindow : Window
    {
        public enum ResizeDirection
        {
            None,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }

        #region private variables

        private bool isDragging = false;
        private bool isMoving = false;
        private bool isResizing = false;
        private Uri imageURI;
        private Point startPoint;
        private Rectangle selectionRectangle = null;
        private Rectangle movingRectangle = null;
        private List<Rectangle> selectedRectangles = new List<Rectangle>();
        private ResizeDirection resizeDirection;
        #endregion

        #region Public methods

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Private methods

        private void OnSelectImage(object sender, RoutedEventArgs e)
        {
            try
            {
                //CLearing the previous work
                Canvas?.Children?.Clear();

                // Open a dialog box to select an image file
                OpenFileDialog openFileDialog = new OpenFileDialog();

                //The Filter property of the OpenFileDialog is set to allow the user to select only image files with extensions .png, .jpeg and .jpg.
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

                //The InitialDirectory property is set to the "My Pictures" folder of the user's 
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                List<Rect> loadedRectangles = new List<Rect>();

                // If the user selects a file, the ShowDialog method returns true.
                if (openFileDialog.ShowDialog() == true)
                {

                    // Load the image URI from the JSON file (if it exists)
                    string jsonFilePath = Path.ChangeExtension(openFileDialog.FileName, "json");
                    if (File.Exists(jsonFilePath))
                    {
                        string json = File.ReadAllText(jsonFilePath);
                        try
                        {

                            Data data = JsonSerializer.Deserialize<Data>(json);

                            imageURI = (Uri)data.imageURI;
                            loadedRectangles = data.rect;
                           

                        }
                        catch (JsonException ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                        catch (ArgumentNullException ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        imageURI = new Uri(openFileDialog.FileName);
                    }

                   
                  

                    // Load the selected image file into a BitmapImage object
                    BitmapImage bitmapImage = new BitmapImage(imageURI);

                    // Create a new Image object and set its Source property to the BitmapImage object
                    Image image = new Image();
                    image.Source = bitmapImage;

                    // Add the Image object as a child of the Canvas object
                    Canvas.Children.Add(image);

                    // Remove the "Select Image" button
                    ((Button)sender).Visibility = Visibility.Collapsed;

                    // show the "Save Image Button" button
                    SaveImageButton.Visibility = Visibility.Visible;

                    // Calculate the aspect ratio of the selected image and the ScrollViewer object
                    double widthRatio = ScrollViewer.ActualWidth / bitmapImage.PixelWidth;
                    double heightRatio = ScrollViewer.ActualHeight / bitmapImage.PixelHeight;
                    double ratio = Math.Min(widthRatio, heightRatio);

                    // Resize the Canvas object to fit the selected image while maintaining its aspect ratio
                    Canvas.Width = bitmapImage.PixelWidth * ratio;
                    Canvas.Height = bitmapImage.PixelHeight * ratio;

                    // Set the Width and Height properties of the Image object to match the Canvas object
                    image.Width = Canvas.Width;
                    image.Height = Canvas.Height;

                    foreach (Rect rect in loadedRectangles)
                    {
                        Rectangle rectangle = new Rectangle()
                        {
                            Width = rect.Width,
                            Height = rect.Height,
                            Stroke = Brushes.Red,
                            StrokeThickness = 3,
                            Opacity = 0.5
                        };
                        Canvas.SetLeft(rectangle, rect.X);
                        Canvas.SetTop(rectangle, rect.Y);
                        Canvas.Children.Add(rectangle);
                    } 
                }
            }
            catch (ArgumentException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (IOException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            } 
        }

        private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                if (Canvas?.Children?.Count > 0 && e?.ButtonState == MouseButtonState.Pressed)
                {
                    Rectangle clickedRectangle = e.Source as Rectangle;
                    if (clickedRectangle != null)
                    {
                        if (!selectedRectangles.Contains(clickedRectangle))
                        {
                            clickedRectangle.Stroke = Brushes.Red;
                            selectedRectangles.Add(clickedRectangle);

                            // Start moving the rectangle
                            isMoving = true;
                            movingRectangle = clickedRectangle;
                            startPoint = e.GetPosition(Canvas);

                            // Check if the click was on the border of the rectangle to resize it
                            Point point = e.GetPosition(clickedRectangle);
                            double borderSize = 10; // The border size in pixels

                            // Check if the click was on any corner of the rectangle to resize it
                            if (point.X < borderSize && point.Y < borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.TopLeft;
                            }
                            else if (point.X > clickedRectangle.ActualWidth - borderSize && point.Y < borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.TopRight;
                            }
                            else if (point.X > clickedRectangle.ActualWidth - borderSize && point.Y > clickedRectangle.ActualHeight - borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.BottomRight;
                            }
                            else if (point.X < borderSize && point.Y > clickedRectangle.ActualHeight - borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.BottomLeft;
                            }
                            // Check if the click was on any edge of the rectangle to resize it
                            else if (point.X < borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.Left;
                            }
                            else if (point.X > clickedRectangle.ActualWidth - borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.Right;
                            }
                            else if (point.Y < borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.Top;
                            }
                            else if (point.Y > clickedRectangle.ActualHeight - borderSize)
                            {
                                isMoving = false;
                                isResizing = true;
                                resizeDirection = ResizeDirection.Bottom;
                            }

                            // show the "Delete Selected Rectangles" button
                            DeleteRectangleButton.Visibility = Visibility.Visible;

                            // show the "Color change" button
                            ColorRectangleButton.Visibility = Visibility.Visible;
                        }
                        else if (clickedRectangle.Stroke == Brushes.Red)
                        {
                            clickedRectangle.Stroke = Brushes.Black;
                            selectedRectangles.Remove(clickedRectangle);
                            isMoving = false;
                            if (selectedRectangles.Count == 0)
                            {
                                DeleteRectangleButton.Visibility = Visibility.Collapsed;
                                ColorRectangleButton.Visibility = Visibility.Collapsed;
                            }
                        }

                    }
                    else
                    {
                        startPoint = e.GetPosition(Canvas);
                        isDragging = true;
                        selectionRectangle = new Rectangle
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 3,
                            Fill = Brushes.LightGray,
                            Opacity = 0.5

                        };

                        Canvas.Children.Add(selectionRectangle);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (Canvas?.Children?.Count > 0 && e?.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPoint = e.GetPosition(Canvas);

                    Image image = (Image)Canvas.Children[0];
                    //Image image = (Image)Canvas.Children.FirstOrDefault();
                    double imageLeft = Canvas.GetLeft(image);
                    double imageTop = Canvas.GetTop(image);
                    double imageRight = imageLeft + image.ActualWidth;
                    double imageBottom = imageTop + image.ActualHeight;

                    if (isDragging)
                    {

                        // Calculate the position and size of the selection rectangle based on the start point and the current point
                        double x = Math.Min(startPoint.X, currentPoint.X);
                        double y = Math.Min(startPoint.Y, currentPoint.Y);
                        double width = Math.Abs(startPoint.X - currentPoint.X);
                        double height = Math.Abs(startPoint.Y - currentPoint.Y);

                        // Check if the selection rectangle extends beyond the bounds of the image, and adjust it if necessary
                        if (x < imageLeft)
                        {
                            width -= (imageLeft - x);
                            x = imageLeft;
                        }
                        if (y < imageTop)
                        {
                            height -= (imageTop - y);
                            y = imageTop;
                        }
                        if (x + width > imageRight) width = imageRight - x;

                        if (y + height > imageBottom) height = imageBottom - y;

                        // Update the position and size of the selection rectangle
                        Canvas.SetLeft(selectionRectangle, x);
                        Canvas.SetTop(selectionRectangle, y);
                        selectionRectangle.Width = width;
                        selectionRectangle.Height = height;

                    }
                    else if (isMoving && movingRectangle != null)
                    {
                        // Moving a rectangle
                        double dx = currentPoint.X - startPoint.X;
                        double dy = currentPoint.Y - startPoint.Y;

                        double newLeft = Canvas.GetLeft(movingRectangle) + dx;
                        double newTop = Canvas.GetTop(movingRectangle) + dy;

                        // Checking to not to move the selection rectangle beyond the bounds of the image, and adjust it if necessary
                        if (newLeft < 0) newLeft = 0;
                        if (newTop < 0) newTop = 0;
                        if (newLeft + movingRectangle.ActualWidth > Canvas.ActualWidth) newLeft = Canvas.ActualWidth - movingRectangle.ActualWidth;
                        if (newTop + movingRectangle.ActualHeight > Canvas.ActualHeight) newTop = Canvas.ActualHeight - movingRectangle.ActualHeight;

                        // Update the position of the selected rectangle
                        Canvas.SetLeft(movingRectangle, newLeft);
                        Canvas.SetTop(movingRectangle, newTop);

                        // Update the start point to the current point
                        startPoint = currentPoint;

                    } 
                    else if (isResizing && movingRectangle != null)
                    {
                        Point currentPosition = e.GetPosition(Canvas);
                        double horizontalChange = currentPosition.X - startPoint.X;
                        double verticalChange = currentPosition.Y - startPoint.Y;

                        double newWidth = movingRectangle.Width;
                        double newHeight = movingRectangle.Height;

                        // each of the resizing cases includes a check to ensure that the new position and size of the rectangle
                        switch (resizeDirection)
                        {
                            case ResizeDirection.TopLeft:
                                double left = Canvas.GetLeft(movingRectangle) + horizontalChange;
                                double top = Canvas.GetTop(movingRectangle) + verticalChange;
                                if (left >= 0 && left + movingRectangle.Width <= Canvas.ActualWidth)
                                {
                                    Canvas.SetLeft(movingRectangle, left);
                                    movingRectangle.Width -= horizontalChange;
                                }
                                if (top >= 0 && top + movingRectangle.Height <= Canvas.ActualHeight)
                                {
                                    Canvas.SetTop(movingRectangle, top);
                                    movingRectangle.Height -= verticalChange;
                                }
                                break;
                            case ResizeDirection.TopRight:
                                double top1 = Canvas.GetTop(movingRectangle) + verticalChange;
                                if (Canvas.GetLeft(movingRectangle) + movingRectangle.Width + horizontalChange <= Canvas.ActualWidth && top1 >= 0 && top1 + movingRectangle.Height <= Canvas.ActualHeight)
                                {
                                    movingRectangle.Width += horizontalChange;
                                    Canvas.SetTop(movingRectangle, top1);
                                    movingRectangle.Height -= verticalChange;
                                }
                                break;
                            case ResizeDirection.BottomRight:
                                if (Canvas.GetLeft(movingRectangle) + movingRectangle.Width + horizontalChange <= Canvas.ActualWidth && Canvas.GetTop(movingRectangle) + movingRectangle.Height + verticalChange <= Canvas.ActualHeight)
                                {
                                    movingRectangle.Width += horizontalChange;
                                    movingRectangle.Height += verticalChange;
                                }
                                break;
                            case ResizeDirection.BottomLeft:
                                double left1 = Canvas.GetLeft(movingRectangle) + horizontalChange;
                                if (left1 >= 0 && left1 + movingRectangle.Width <= Canvas.ActualWidth && Canvas.GetTop(movingRectangle) + movingRectangle.Height + verticalChange <= Canvas.ActualHeight)
                                {
                                    Canvas.SetLeft(movingRectangle, left1);
                                    movingRectangle.Width -= horizontalChange;
                                    movingRectangle.Height += verticalChange;
                                }
                                break;
                            case ResizeDirection.Left:
                                double left2 = Canvas.GetLeft(movingRectangle) + horizontalChange;
                                if (left2 >= 0 && left2 + movingRectangle.Width <= Canvas.ActualWidth)
                                {
                                    Canvas.SetLeft(movingRectangle, left2);
                                    movingRectangle.Width -= horizontalChange;
                                }
                                break;
                            case ResizeDirection.Right:
                                if (Canvas.GetLeft(movingRectangle) + movingRectangle.Width + horizontalChange <= Canvas.ActualWidth) movingRectangle.Width += horizontalChange;  
                                break;
                            case ResizeDirection.Top:
                                double top2 = Canvas.GetTop(movingRectangle) + verticalChange;
                                if (top2 >= 0 && top2 + movingRectangle.Height <= Canvas.ActualHeight)
                                {
                                    Canvas.SetTop(movingRectangle, top2);
                                    movingRectangle.Height -= verticalChange;
                                }
                                break;
                            case ResizeDirection.Bottom:
                                if (Canvas.GetTop(movingRectangle) + movingRectangle.Height + verticalChange <= Canvas.ActualHeight) movingRectangle.Height += verticalChange;
                                break;
                            default:
                                break;
                        }

                        startPoint = currentPosition;
                    }

                }
            } 
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void OnImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Canvas.Children.Count > 0 && e.ButtonState == MouseButtonState.Released)
                {
                    if (isDragging) isDragging = false;
                    else if (isMoving || isResizing)
                    {
                        isMoving = false;
                        isResizing = false;
                        movingRectangle = null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void OnDeleteSelectedRectangles(object sender, RoutedEventArgs e)
        {
            try
            {
                // Remove the selected rectangles from the canvas
                foreach (Rectangle selectedRectangle in selectedRectangles) Canvas.Children.Remove(selectedRectangle);

                selectedRectangles.Clear();

                // Remove the "Delete Rectangle" button
                ((Button)sender).Visibility = Visibility.Collapsed;

                if (selectedRectangles.Count == 0) ColorRectangleButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void OnFeedbackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new Window object to serve as the dialog box
                Window dialog = new Window
                {
                    Title = "Feedback",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ShowInTaskbar = false,
                    ResizeMode = ResizeMode.NoResize,
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(10),
                        Children =
            {
                new TextBlock
                {
                    Text = "Please enter your feedback message:",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                },
                new System.Windows.Controls.TextBox
                {
                    Name = "txtFeedback",
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(0, 10, 0, 0),
                    Children =
                    {
                        new Button
                        {
                            Content = "Cancel",
                            Width = 80,
                            Margin = new Thickness(10),
                            IsCancel = true
                        },
                        new Button
                        {
                            Content = "OK",
                            Width = 80,
                            Margin = new Thickness(10),
                            IsDefault = true
                        }
                    }
                }
            }
                    }
                };

                // Show the dialog box and wait for the user to click a button
                bool? result = dialog.ShowDialog();

                // If the user clicked the OK button, retrieve the feedback message
                if (result == true)
                {
                    System.Windows.Controls.TextBox txtFeedback = dialog.FindName("txtFeedback") as System.Windows.Controls.TextBox;
                    if (txtFeedback != null)
                    {
                        string feedbackMessage = txtFeedback.Text;

                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // string filePath = "TextFile1.txt"; // specify the file name and extension
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.txt");

                if (File.Exists(filePath)) // check if file exists
                {
                    string fileContent = File.ReadAllText(filePath); // read the entire content of the file

                    // create a new dialog box window and set its properties
                    Window dialogBox = new Window();
                    dialogBox.Title = "File Content";
                   // dialogBox.SizeToContent = SizeToContent.WidthAndHeight;
                    dialogBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    // create a scroll viewer to display the file content
                    ScrollViewer scrollViewer = new ScrollViewer();
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.Margin = new Thickness(10);

                    // create a text block to display the file content
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = fileContent;
                    textBlock.Margin = new Thickness(10);

                    // add the text block to the scroll viewer's content
                    scrollViewer.Content = textBlock;

                    // add the scroll viewer to the dialog box's content
                    dialogBox.Content = scrollViewer;

                    // display the dialog box
                    dialogBox.ShowDialog();
                }
                else
                {
                    MessageBox.Show("File not found!", "Error"); // if file does not exist, display an error message in a dialog box
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }



        private void OnSaveImage(object sender, RoutedEventArgs e)
        {
            try
            {
                // show the "select image" button
                SelectImageButton.Visibility = Visibility.Visible;

                // Create a RenderTargetBitmap of the Canvas
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(Canvas);

                // Create a PngBitmapEncoder and add the RenderTargetBitmap to it
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                // Create a list to store the rectangles
                List<Rect> rectangles = new List<Rect>();

                // Get all the rectangle objects on the canvas and add their data to the list
                foreach (UIElement element in Canvas.Children)
                {
                    if (element is Rectangle rectangle)
                    {
                        rectangles.Add(new Rect
                        {
                            X = Canvas.GetLeft(rectangle),
                            Y = Canvas.GetTop(rectangle),
                            Width = rectangle.Width,
                            Height = rectangle.Height,
                            //  Fill = rectangle.Fill
                        });
                    }
                }

                // Save the image
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Save the image file
                    using (Stream stream = File.Create(saveFileDialog.FileName))
                    {
                        pngEncoder.Save(stream);
                    }

                    var json = new { imageURI, rect = rectangles };
                    // Save the rectangle data to a JSON file
                    string jsonString = JsonSerializer.Serialize(json);
                    File.WriteAllText(Path.ChangeExtension(saveFileDialog.FileName, "json"), jsonString);

                }

            }
            catch (ArgumentException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (IOException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void OnColorSelectedRectangles(object sender, RoutedEventArgs e)
        {
            try
            {
                Brush brush = GetSelectedColor();

                foreach (Rectangle rectangle in selectedRectangles) rectangle.Fill = brush;

                // Remove the "Color change" button
                ((Button)sender).Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            
        }

        private Brush GetSelectedColor()
        {
            try
            {
                ColorDialog colorDialog = new ColorDialog();
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Drawing.Color color = colorDialog.Color;
                    return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
                }
                return Brushes.Black; // Return a default color if no color is selected
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return Brushes.Black; // Return a default color if no color is selected
            }

        }
        #endregion
    }

}
