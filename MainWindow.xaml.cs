using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;
using System.Xml.Linq;
using Verde.Utility;

namespace Verde
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExternalCacheDatabase dbImageCache;
        private CanvasManager mngCanvas;
        private ElementsContainer ecAllPages;
        private Point posCurrent;
        private bool bDisplaySettings = false;
        private static double nStartPos = 56;
        private string strHomeUrl = "http://pya.cc/ipn/index.php?page=";
        private Image imgSplash;
        private AnimationGif gifLoading;

        public MainWindow()
        {
            InitializeComponent();

            this.dbImageCache = new ExternalCacheDatabase();
            this.mngCanvas = new CanvasManager();
            this.ecAllPages = new ElementsContainer(strHomeUrl);
            this.posCurrent = new Point(16, MainWindow.nStartPos);

            this.mngCanvas.Add("Main", this.canvasMain, CanvasManager.Order.ORDER_FOREGROUND);
            this.mngCanvas.Add("Settings", this.canvasSettings, CanvasManager.Order.ORDER_BACKGROUND);

            //var p = new Paragraph();
            //p.Inlines.Add("この文字は");
            //var span = new Span { Foreground = Brushes.Red };
            //span.Inlines.Add("赤");
            //p.Inlines.Add(span);
            //p.Inlines.Add("です。");
            //this.rtboxSettings.Document.Blocks.Add(p);

            this.imgSplash = ImageProcessing.GetStillImageFromResource("Verde.Resources.Verde.png");
            ImageProcessing.SetImageToCenter(this.canvasMain, this.imgSplash);

            this.gifLoading = new AnimationGif("Verde.Resources.loading.gif", 100);
            ImageProcessing.SetImage(this.canvasMain, new Point(10, 10), this.gifLoading.FrameImage);
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            Cursor curCurrent = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            //this.ecAllPages.ImportPage(1);
            this.ecAllPages.ImportPages(1, 10);

            // Set Background Color
            SolidColorBrush brsBackground = new SolidColorBrush();
            brsBackground.Color = this.ecAllPages.BackgroundColor;
            this.canvasMain.Background = brsBackground;

            Mouse.OverrideCursor = curCurrent;
        }

        private void CheckPath(string strPath)
        {
            Console.WriteLine(strPath);
        }

        private void DrawThumbImage(BitmapImage imgThumb)
        {
            // TENTATIVE
            if (this.posCurrent.X > 16) return;

            Image image = new Image();
            image.Source = imgThumb;
            image.Width = imgThumb.PixelWidth;
            image.Height = imgThumb.PixelHeight;

            Canvas.SetLeft(image, this.posCurrent.X);
            Canvas.SetTop(image, this.posCurrent.Y);
            this.posCurrent.Y += image.Height + 16;

            Rectangle rect = new Rectangle();
            rect.Name = "rect" + imgThumb.UriSource.Segments.Last<String>();
            rect.Width = this.canvasMain.Width - 16;
            rect.Height = image.Height + 16;
            rect.Stroke = Brushes.Gray;
            rect.RadiusX = 4;
            rect.RadiusY = 4;
            Canvas.SetLeft(rect, this.posCurrent.X - 8);
            Canvas.SetTop(rect, this.posCurrent.Y - image.Height - 24);
            this.canvasMain.Children.Add(rect);

            this.canvasMain.Children.Add(image);

            if (this.posCurrent.Y > this.canvasMain.ActualHeight - image.Height) {
                this.posCurrent.X += image.Width + 8;
                this.posCurrent.Y = MainWindow.nStartPos;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (bDisplaySettings) {
                this.mngCanvas.Raise("Main");
            } else {
                this.mngCanvas.Raise("Settings");
            }
            bDisplaySettings = !bDisplaySettings;
        }
    }
}
