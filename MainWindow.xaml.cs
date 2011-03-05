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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Xml.Linq;
using Sgml;

namespace Verde
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExternalCacheDatabase dbCache;
        private Point posCurrent;

        public MainWindow()
        {
            InitializeComponent();

            this.dbCache = new ExternalCacheDatabase();
            this.posCurrent = new Point(16, 16);
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            using (var stream = new WebClient().OpenRead("http://pya.cc/ipn/index.php"))
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                var xml = ParseHtml(sr);

                XNamespace ns = "http://www.w3.org/1999/xhtml";

                string strStyle = xml.Root.Element(ns + "body").Attribute("style").Value;
                string strBackground = "background-color:#";
                if (String.Compare(strBackground, 0, strStyle, 0, strBackground.Length, true) == 0) {
                    string strBgValue = strStyle.Substring(strBackground.Length, strStyle.Length - strBackground.Length);
                    SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                    mySolidColorBrush.Color = Color.FromRgb(Convert.ToByte(strBgValue.Substring(0, 2), 16), Convert.ToByte(strBgValue.Substring(2, 2), 16), Convert.ToByte(strBgValue.Substring(4, 2), 16));
                    this.canvasMain.Background = mySolidColorBrush;
                }

                foreach (var item in xml.Descendants(ns + "img"))
                {
                    XAttribute attr = item.Attribute("class");
                    if (attr != null && attr.Value == "thumb")
                    {
                        string strUrl = item.Attribute("src").Value;
                        if (String.Compare("http:", 0, strUrl, 0, 5) != 0)
                        {
                            strUrl = "http://pya.cc" + strUrl;
                        }
                        this.dbCache.GetCache(strUrl, CheckPath);
                        this.dbCache.GetImageCache(strUrl, DrarThumbImage);
                    }

                }
            }
        }

        static XDocument ParseHtml(TextReader reader)
        {
            using (var sgmlReader = new SgmlReader { DocType = "HTML", CaseFolding = CaseFolding.ToLower })
            {
                sgmlReader.InputStream = reader;
                return XDocument.Load(sgmlReader);
            }
        }

        private void DrarThumbImage(BitmapImage imgThumb)
        {
            Image image = new Image();
            image.Source = imgThumb;
            image.Width = imgThumb.PixelWidth;
            image.Height = imgThumb.PixelHeight;

            Canvas.SetLeft(image, this.posCurrent.X);
            Canvas.SetTop(image, this.posCurrent.Y);
            this.posCurrent.Y += image.Height + 8;
            if (this.posCurrent.Y > this.canvasMain.ActualHeight - image.Height)
            {
                this.posCurrent.X += image.Width + 8;
                this.posCurrent.Y = 16;
            }

            this.canvasMain.Children.Add(image);
        }

        private void CheckPath(string strPath)
        {
            Console.WriteLine(strPath);
        }
    }
}
