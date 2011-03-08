using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private ExternalCacheDatabase dbCache;
        private Point posCurrent;
        private bool bLargeThumb = true;
        private Storyboard myStoryboard;

        public MainWindow()
        {
            InitializeComponent();

            this.dbCache = new ExternalCacheDatabase();
            this.posCurrent = new Point(16, 16);
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            using (var stmHome = new WebClient().OpenRead("http://pya.cc/ipn/index.php"))
            using (var srHome = new StreamReader(stmHome, Encoding.UTF8))
            {
                var xmlHome = HtmlParser.Parse(srHome);

                XNamespace ns = "http://www.w3.org/1999/xhtml";

                // Set Background Color from style
                string strStyle = xmlHome.Root.Element(ns + "body").Attribute("style").Value;
                string strBackground = "background-color:#";
                if (String.Compare(strBackground, 0, strStyle, 0, strBackground.Length, true) == 0) {
                    string strBgValue = strStyle.Substring(strBackground.Length, strStyle.Length - strBackground.Length);
                    if (strBgValue.Length >= 6) {
                        strBgValue = strBgValue.Substring(0, 6);
                        if (StringProcessing.IsHexadecimal(strBgValue) == true) {
                            SolidColorBrush brsBackground = new SolidColorBrush();
                            brsBackground.Color = StringProcessing.ConvertFromHexStringToColor(strBgValue);
                            this.canvasMain.Background = brsBackground;
                        }
                    }
                }

                // Extract Thumbnails
                foreach (var item in xmlHome.Descendants(ns + "img")) {
                    XAttribute attr = item.Attribute("class");
                    if (attr != null && attr.Value == "thumb") {
                        string strUrl = item.Attribute("src").Value;
                        if (String.Compare("http:", 0, strUrl, 0, 5) != 0) {
                            if (bLargeThumb == true) {
                                /* 現状決め打ち "/i"を除外する */
                                strUrl = strUrl.Substring(2);
                            }
                            strUrl = "http://pya.cc" + strUrl;
                        }
                        this.dbCache.GetCache(strUrl, CheckPath);
                        this.dbCache.GetImageCache(strUrl, this.DrawThumbImage);
                    }
                }
            }
        }

        private void DrawThumbImage(BitmapImage imgThumb)
        {
            Image image = new Image();
            image.Source = imgThumb;
            image.Width = imgThumb.PixelWidth;
            image.Height = imgThumb.PixelHeight;

            Canvas.SetLeft(image, this.posCurrent.X);
            Canvas.SetTop(image, this.posCurrent.Y);
            this.posCurrent.Y += image.Height + 8;
            if (this.posCurrent.Y > this.canvasMain.ActualHeight - image.Height) {
                this.posCurrent.X += image.Width + 8;
                this.posCurrent.Y = 16;
            }

            this.canvasMain.Children.Add(image);
        }

        private void CheckPath(string strPath)
        {
            Console.WriteLine(strPath);
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(10)), FillBehavior.Stop);
            myDoubleAnimation.Completed += new EventHandler(myDoubleAnimation_Completed);
            canvasSettings.BeginAnimation(Rectangle.OpacityProperty, myDoubleAnimation);
            canvasSettings.Visibility = Visibility.Visible;
        }

        void myDoubleAnimation_Completed(object sender, EventArgs e)
        {
            canvasSettings.Opacity = 1.0;
        }
    }
}
