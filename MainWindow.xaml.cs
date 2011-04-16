using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
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
        private static Point posLoadingGif = new Point(10, 10);
        private string strHomeUrl = "http://pya.cc/ipn/index.php?page=";
        private Image imgSplash;
        private Cursor curBackingStore;
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

            this.imgSplash = ImageProcessing.GetStillImageFromResource("Verde.Resources.Verde.png");
            ImageProcessing.SetImageToCenter(this.canvasMain, this.imgSplash);

            this.gifLoading = new AnimationGif("Verde.Resources.loading.gif");
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(DoImportPages);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnCompletedImport);

            this.curBackingStore = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            ImageProcessing.SetImage(this.canvasMain, MainWindow.posLoadingGif, this.gifLoading.FrameImage);

            worker.RunWorkerAsync();
        }

        private void DoImportPages(object sender, DoWorkEventArgs e)
        {
            this.ecAllPages.ImportPage(1);
            //this.ecAllPages.ImportPages(1, 10);

            // Set Background Color
            //SolidColorBrush brsBackground = new SolidColorBrush();
            //brsBackground.Color = this.ecAllPages.BackgroundColor;
            //this.canvasMain.Background = brsBackground;
        }

        private void OnCompletedImport(object sender, RunWorkerCompletedEventArgs e)
        {
            ImageProcessing.FadeOut(this.gifLoading.FrameImage, 100, null);
            ImageProcessing.FadeOut(this.imgSplash, 500, this.OnCompletedFader);
            Mouse.OverrideCursor = this.curBackingStore;
        }

        private void OnCompletedFader(object sender, EventArgs e)
        {
            this.imgSplash = null;

            // TEST
            for (var i = 0; i < 7; i++) {
                Element element = this.ecAllPages.Elements[i];
                string strUrl = "http://pya.cc" + element.UrlThumbnail;
                this.dbImageCache.GetCache(strUrl, this.CheckPath);
                this.dbImageCache.GetImageCache(strUrl, this.DrawThumbImage);

                Canvas.SetLeft(element.Header, this.posCurrent.X + 150);
                Canvas.SetTop(element.Header, this.posCurrent.Y);
                this.canvasMain.Children.Add(element.Header);

                this.posCurrent.Y += rtb.Height + 16;
            }
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
            //this.posCurrent.Y += image.Height + 16;

            //Rectangle rect = new Rectangle();
            //rect.Name = "rect" + imgThumb.UriSource.Segments.Last<String>();
            //rect.Width = this.canvasMain.Width - 16;
            //rect.Height = image.Height + 16;
            //rect.Stroke = Brushes.Gray;
            //rect.RadiusX = 4;
            //rect.RadiusY = 4;
            //Canvas.SetLeft(rect, this.posCurrent.X - 8);
            //Canvas.SetTop(rect, this.posCurrent.Y - image.Height - 24);
            //this.canvasMain.Children.Add(rect);

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
