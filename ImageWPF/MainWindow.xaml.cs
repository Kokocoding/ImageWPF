using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.Linq;

namespace ImageWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPage();
        }

        private bool mouseDown;
        private bool editS = true;
        private Point mouseXY;
        private System.Drawing.Point bitmapXY;
        private List<Button> dynamicButtons = new List<Button>();
        private int zoomCount = 0;
        private int ButtonIndex = 0;
        private string uploadImagePath;
        private Image OpenImages;
        private Border OpenBorder;

        private void IMG_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Point imagePositionA = uploadedImage.TranslatePoint(new Point(0, 0), BtnCanvas);
            //uploadedImage.RenderTransform = null;
            //Canvas.SetLeft(uploadedImage, 0);
            //Canvas.SetTop(uploadedImage, 0);
            //TranslateTransform transform = uploadedImage.RenderTransform as TranslateTransform;
            //if (transform == null)
            //{
            //    transform = new TranslateTransform();
            //    uploadedImage.RenderTransform = transform;
            //}

            //transform.X = 0;
            //transform.Y = 0;

            Point p = e.GetPosition(BtnCanvas);
            double pixelWidth = uploadedImage.Source.Width;
            double pixelHeight = uploadedImage.Source.Height;
            double x = pixelWidth * p.X / uploadedImage.ActualWidth;
            double y = pixelHeight * p.Y / uploadedImage.ActualHeight;
            bitmapXY = new System.Drawing.Point((int)x, (int)y);
        }
        private void IMG_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            uploadedImage.ReleaseMouseCapture();
        }
        //鼠标按下时的事件，启用捕获鼠标位置并把坐标赋值给mouseXY.
        private void IMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
        }
        //鼠标松开时的事件，停止捕获鼠标位置。
        private void IMG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.ReleaseMouseCapture();
            mouseDown = false;
        }
        //鼠标移动时的事件，当鼠标按下并移动时发生
        private void IMG_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            if (mouseDown)
            {
                Domousemove(img, e);
            }
        }        
        private void Domousemove(ContentControl img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var group = IMG.FindResource("Imageview") as TransformGroup;
            var transform = group.Children[1] as TranslateTransform;
            var position = e.GetPosition(img);
            transform.X -= mouseXY.X - position.X;
            transform.Y -= mouseXY.Y - position.Y;

            foreach (Button button in dynamicButtons)
            {
                var buttonPosition = button.TranslatePoint(new Point(0, 0), img);
                Canvas.SetLeft(button, buttonPosition.X - (mouseXY.X - position.X));
                Canvas.SetTop(button, buttonPosition.Y - (mouseXY.Y - position.Y));
            }

            mouseXY = position;
        }

        // 滑鼠滾輪 放大縮小
        private void IMG_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //var delta = e.Delta * 0.001;
            Point imagePosition = uploadedImage.TranslatePoint(new Point(0, 0), BtnCanvas);
            if (e.Delta > 0)
            {
                uploadedImage.Width = uploadedImage.ActualWidth * 1.2; // 放大圖片的寬度
                uploadedImage.Height = uploadedImage.ActualHeight * 1.2; // 放大圖片的高度

                foreach (Button button in dynamicButtons)
                {
                    var buttonPosition = button.TranslatePoint(new Point(0, 0), uploadedImage);
                    Canvas.SetLeft(button, buttonPosition.X * 1.2 + imagePosition.X);
                    Canvas.SetTop(button, buttonPosition.Y * 1.2 + imagePosition.Y);

                    button.Width = button.ActualWidth * 1.2;
                    button.Height = button.ActualHeight * 1.2;
                }
                ++zoomCount;
            }
            else
            {
                uploadedImage.Width = uploadedImage.ActualWidth / 1.2; // 放大圖片的寬度
                uploadedImage.Height = uploadedImage.ActualHeight / 1.2; // 放大圖片的高度

                foreach (Button button in dynamicButtons)
                {
                    var buttonPosition = button.TranslatePoint(new Point(0, 0), uploadedImage);
                    Canvas.SetLeft(button, buttonPosition.X / 1.2 + imagePosition.X);
                    Canvas.SetTop(button, buttonPosition.Y / 1.2 + imagePosition.Y);

                    button.Width = button.ActualWidth / 1.2;
                    button.Height = button.ActualHeight / 1.2;
                }                
                --zoomCount;
            }
            /*
            //鼠标滑轮事件，得到坐标，放缩函数和滑轮指数，由于滑轮值变化较大所以*0.001.
            //    var img = sender as ContentControl;
            //    if (img == null)
            //    {
            //        return;
            //    }            

            //    var point = e.GetPosition(img);
            //    var group = IMG.FindResource("Imageview") as TransformGroup;            
            //    DowheelZoom(group, point, delta);

            //    var transform = group.Children[0] as ScaleTransform;
            //    if (transform == null)
            //    {
            //        return;
            //    }

            //    // 更新動態新增按鈕的位置和大小
            //    foreach (Button button in dynamicButtons)
            //    {
            //        var buttonPosition = button.TranslatePoint(new System.Windows.Point(0, 0), img);
            //        var scaledPosition = new System.Windows.Point(
            //            buttonPosition.X * transform.ScaleX,
            //            buttonPosition.Y * transform.ScaleY
            //        );
            //        var buttonTransform = new TranslateTransform(
            //            scaledPosition.X - buttonPosition.X,
            //            scaledPosition.Y - buttonPosition.Y
            //        );
            //        button.RenderTransform = new TransformGroup
            //        {
            //            Children = new TransformCollection
            //            {
            //                new ScaleTransform(transform.ScaleX, transform.ScaleY),
            //                buttonTransform
            //            }
            //        };
            //    }

                //private void DowheelZoom(TransformGroup group, System.Windows.Point point, double delta)
                //{
                //    var pointToContent = group.Inverse.Transform(point);
                //    var transform = group.Children[0] as ScaleTransform;
                //    if (transform.ScaleX + delta < 0.1) return;
                //    transform.ScaleX += delta;
                //    transform.ScaleY += delta;
                //    var transform1 = group.Children[1] as TranslateTransform;
                //    transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
                //    transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
                //}

            */
        }

        // 按鈕拖曳
        private string ButtonNames;
        private void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            button.CaptureMouse();
        }
        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button.IsMouseCaptured)
            {
                Point currentPosition = e.GetPosition(BtnCanvas);
                Canvas.SetLeft(button, currentPosition.X - button.ActualWidth / 2);
                Canvas.SetTop(button, currentPosition.Y - button.ActualHeight / 2);
            }
        }
        private void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            button.ReleaseMouseCapture();
        }
        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            Button newButton = e.Data.GetData(typeof(Button)) as Button;
            if (newButton != null)
            {
                Canvas canvas = sender as Canvas;
                Point dropPosition = e.GetPosition(BtnCanvas);

                Button clonedButton = new Button()
                {
                    Width = newButton.ActualWidth,
                    Height = newButton.ActualHeight,
                    Content = newButton.Content,
                    Background = newButton.Background
                };

                Canvas.SetLeft(clonedButton, dropPosition.X - clonedButton.ActualWidth / 2);
                Canvas.SetTop(clonedButton, dropPosition.Y - clonedButton.ActualHeight / 2);

                clonedButton.PreviewMouseDown += PreviewMouseDown;
                clonedButton.PreviewMouseMove += PreviewMouseMove;
                clonedButton.PreviewMouseUp += PreviewMouseUp;

                canvas.Children.Add(clonedButton);
            }
        }

        private void ButtoIndex(object sender, MouseButtonEventArgs e)
        {
            //滑鼠右鍵選定按鈕
            Button button = sender as Button;
            ButtonNames = button.Name;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Button newButton = new Button()
            {
                Width = 50 * Math.Pow(1.2, zoomCount),
                Height = 50 * Math.Pow(1.2, zoomCount),
                Content = ButtonName.Text,
                Name = $"Button_{ButtonIndex++}",
                Tag = ButtonValue.Text
            };
            newButton.PreviewMouseDown += PreviewMouseDown;
            newButton.PreviewMouseMove += PreviewMouseMove;
            newButton.PreviewMouseUp += PreviewMouseUp;
            newButton.PreviewMouseRightButtonDown += ButtoIndex;

            dynamicButtons.Add(newButton);

            Canvas.SetLeft(newButton, 100);
            Canvas.SetTop(newButton, 100);

            BtnCanvas.Children.Add(newButton);
            RegisterName(newButton.Name, newButton);
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = BtnCanvas.FindName(ButtonNames) as Button;
            if (button != null)
            {
                BtnCanvas.Children.Remove(button);
            }
        }
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                uploadImagePath = openFileDialog.FileName;

                // 取得檔案的副檔名
                string fileExtension = Path.GetExtension(uploadImagePath);
                string newFileName = $"{Guid.NewGuid()}{fileExtension}";

                // 移動圖片到 image 資料夾
                string destinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image", newFileName);
                File.Copy(uploadImagePath, destinationPath, true);

                // 設定 Image 控制項的來源為上傳的圖片
                uploadedImage.Source = new BitmapImage(new Uri(uploadImagePath));

                dynamicButtons.Clear();
                ButtonIndex = 0;
                List<Button> buttonsToRemove = BtnCanvas.Children.OfType<Button>().ToList();
                foreach (Button button in buttonsToRemove)
                {
                    BtnCanvas.Children.Remove(button);
                }
            }
            LoadPage();
        }
        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (uploadedImage != null) uploadedImage.Source = null;
            if (OpenImages != null) OpenImages.Source = null;
            imageWrapPanel.Children.Clear();
            File.Delete(uploadImagePath);
            LoadPage();
        }
        private void SaveInfo_Click(object sender, RoutedEventArgs e)
        {
            if (uploadedImage.Source != null)
            {
                //按鈕資訊
                List<ButtonInfo> buttonInfos = new List<ButtonInfo>();
                foreach (Button button in dynamicButtons)
                {
                    var buttonPosition = button.TranslatePoint(new Point(0, 0), uploadedImage);

                    buttonInfos.Add(new ButtonInfo
                    {
                        Name = button.Name,
                        Content = button.Content.ToString(),
                        Height = (int)button.Height,
                        Width = (int)button.Width,
                        Position = new Point(buttonPosition.X, buttonPosition.Y),
                        Tag = button.Tag.ToString() ?? ""
                    });
                }

                //圖片資訊
                List<ImageInfo> ImageInfos = new List<ImageInfo>();
                ImageInfos.Add(new ImageInfo
                {
                    Name = Path.GetFileNameWithoutExtension(uploadedImage.Source.ToString()),
                    Height = (int)uploadedImage.ActualHeight,
                    Width = (int)uploadedImage.ActualWidth,
                    ButtonInfos = buttonInfos,
                    ZoomCount = zoomCount
                });

                // 將變數轉換為 JSON 字串
                string jsonString = JsonSerializer.Serialize(ImageInfos);

                // 指定 JSON 檔案路徑
                string jsonFilePath = $"ImageJson/{Path.GetFileNameWithoutExtension(uploadedImage.Source.ToString())}.json";

                // 將 JSON 字串寫入 JSON 檔案
                File.WriteAllText(jsonFilePath, jsonString);
            }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            Button buttonC = sender as Button;

            AddButton.IsEnabled = !editS;
            RemoveButton.IsEnabled = !editS;
            SaveButton.IsEnabled = !editS;

            if (editS)
            {                
                buttonC.Content = "編輯";

                foreach (Button button in dynamicButtons)
                {
                    button.PreviewMouseDown -= PreviewMouseDown;
                    button.PreviewMouseMove -= PreviewMouseMove;
                    button.PreviewMouseUp -= PreviewMouseUp;
                    button.PreviewMouseRightButtonDown -= ButtoIndex;
                    button.Click += Button_Click;
                }
                editS = false;
            }
            else
            {
                buttonC.Content = "取消編輯";                

                foreach (Button button in dynamicButtons)
                {
                    button.PreviewMouseDown += PreviewMouseDown;
                    button.PreviewMouseMove += PreviewMouseMove;
                    button.PreviewMouseUp += PreviewMouseUp;
                    button.PreviewMouseRightButtonDown += ButtoIndex;
                    button.Click -= Button_Click;
                }
                editS = true;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            MessageBox.Show(button.Tag.ToString());
        }

        //圖片列表
        private List<string> imagePaths = new List<string>();
        private int currentPage = 0;
        private int imagesPerPage = 8;
        private void LoadPage()
        {
            // 添加圖片路徑到 imagePaths 指定圖片資料夾路徑
            string imagesFolderPath = AppDomain.CurrentDomain.BaseDirectory + "Image";
            // 獲取圖片資料夾中的圖片檔案路徑
            string[] imageFiles = Directory.GetFiles(imagesFolderPath);

            // 清空 imagePaths 列表
            imagePaths.Clear();

            // 將圖片檔案路徑添加到 imagePaths 列表中
            imagePaths.AddRange(imageFiles);

            // 清除 WrapPanel 中的圖片
            imageWrapPanel.Children.Clear();

            // 計算起始索引和結束索引
            int startIndex = currentPage * imagesPerPage;
            int endIndex = Math.Min(startIndex + imagesPerPage, imagePaths.Count);

            // 加入圖片到 WrapPanel 中
            for (int i = startIndex; i < endIndex; i++)
            {
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(imagePaths[i]));
                image.Width = 100; // 設定圖片寬度
                image.Height = 100; // 設定圖片高度
                image.Margin = new Thickness(5);

                // 創建 Border 控制項並設定外框樣式
                Border border = new Border();
                border.BorderBrush = Brushes.Blue; // 外框顏色
                border.BorderThickness = new Thickness(0); // 外框寬度

                border.Child = image;

                imageWrapPanel.Children.Add(border);

                // 添加 Click 事件處理程序
                border.MouseLeftButtonUp += OpenImage;
            }

            // 更新左右切換頁面按鈕的可見性
            prevButton.IsEnabled = currentPage > 0;
            nextButton.IsEnabled = endIndex < imagePaths.Count;
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                LoadPage();
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)imagePaths.Count / imagesPerPage);
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                LoadPage();
            }
        }

        private void OpenImage(object sender, MouseButtonEventArgs e)
        {
            //選取圖片邊框取消
            OpenBorder = sender as Border;
            foreach (Border b in FindBorders(imageWrapPanel))
            {
                b.BorderThickness = new Thickness(0);
            }
            //選取圖片邊框顯示
            OpenBorder.BorderThickness = new Thickness(2);

            //清除按鈕 image元件留著
            // 清除動態按鈕集合
            dynamicButtons.Clear();
            ButtonIndex = 0;
            List<Button> buttonsToRemove = BtnCanvas.Children.OfType<Button>().ToList();
            foreach (Button button in buttonsToRemove)
            {
                BtnCanvas.Children.Remove(button);
            }

            //圖片
            OpenImages = OpenBorder.Child as Image;
            Uri imageSourceUri = (OpenImages.Source as BitmapImage)?.UriSource;
            uploadImagePath = imageSourceUri.AbsolutePath;
            uploadedImage.Source = new BitmapImage(new Uri(uploadImagePath));

            // 設定 uploadedImage 控制項初始化
            Point imagePosition = uploadedImage.TranslatePoint(new Point(0, 0), BtnCanvas);
            Canvas.SetLeft(uploadedImage, 0);
            Canvas.SetTop(uploadedImage, 0);

            BitmapImage originalImage = new BitmapImage(new Uri(uploadImagePath)); // 建立 BitmapImage 物件，以獲取原始圖片的寬度和高度
            uploadedImage.Width = originalImage.Width;
            uploadedImage.Height = originalImage.Height;
            zoomCount = 0;            

            //圖片按鈕資料JSON
            string fileName = Path.GetFileNameWithoutExtension(Path.GetFileName(imageSourceUri.LocalPath));
            string jsonFilePath = $"ImageJson/{fileName}.json";

            if (File.Exists(jsonFilePath))
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                List<ImageInfo> imageInfos = JsonSerializer.Deserialize<List<ImageInfo>>(jsonString);                
                foreach (ImageInfo imageInfo in imageInfos)
                {
                    //存取屬性
                    string name = imageInfo.Name;                    
                    uploadedImage.Width = imageInfo.Width;
                    uploadedImage.Height = imageInfo.Height;
                    zoomCount = imageInfo.ZoomCount;

                    // 存取按鈕資訊
                    List<ButtonInfo> buttonInfos = imageInfo.ButtonInfos;
                    foreach (ButtonInfo buttonInfo in buttonInfos)
                    {
                        Button newButton = new Button()
                        {
                            Width = buttonInfo.Width,
                            Height = buttonInfo.Height,
                            Content = buttonInfo.Content,
                            Name = buttonInfo.Name,
                            Tag = buttonInfo.Tag,
                        };
                        newButton.PreviewMouseDown += PreviewMouseDown;
                        newButton.PreviewMouseMove += PreviewMouseMove;
                        newButton.PreviewMouseUp += PreviewMouseUp;
                        newButton.PreviewMouseRightButtonDown += ButtoIndex;

                        dynamicButtons.Add(newButton);
                        BtnCanvas.Children.Add(newButton);

                        //按鈕定位
                        Point point = new Point(buttonInfo.Position.X + imagePosition.X, buttonInfo.Position.Y + imagePosition.Y);
                        Point relativePosition = newButton.TranslatePoint(point, BtnCanvas); // 新按鈕移動到uploadedImage相對位置
                        Canvas.SetLeft(newButton, relativePosition.X);
                        Canvas.SetTop(newButton, relativePosition.Y);
                        
                        //按鈕是否註冊過
                        bool isButtonRegistered = dynamicButtons.Any(button => button.Name == newButton.Name);
                        if (!isButtonRegistered)
                        {
                            RegisterName(newButton.Name, newButton);
                        }
                    }
                }
            }
        }
        // 使用 VisualTreeHelper 找到指定視覺樹節點下的所有 Border 控制項
        private List<Border> FindBorders(DependencyObject parent)
        {
            List<Border> borders = new List<Border>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Border border)
                {
                    borders.Add(border);
                }
                else
                {
                    List<Border> childBorders = FindBorders(child);
                    borders.AddRange(childBorders);
                }
            }

            return borders;
        }
    }

    public class ImageInfo
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int ZoomCount { get; set; }
        public List<ButtonInfo> ButtonInfos { get; set; }
    }
    public class ButtonInfo
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Point Position { get; set; }
        public string Tag { get; set; }
    }
}
