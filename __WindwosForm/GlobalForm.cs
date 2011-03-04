using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using Sgml;
using System.Net;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace Verde
{
    public partial class GlobalForm : Form
    {
        private ExternalCacheDatabase dbCache;
        private int posCurrent = 0;
        //private Canvas canvasMain = null;

        public GlobalForm()
        {
            InitializeComponent();

            //this.canvasMain = new Canvas();
            //this.Controls.Add(this.canvasMain);
            this.dbCache = new ExternalCacheDatabase();
        }

        private void CheckPath(string strPath)
        {
            Console.WriteLine(strPath);
        }

        private void DrarThumbImage(BitmapImage imgThumb)
        {
            Image image = new Image();
            image.Source = imgThumb;
            image.Width = imgThumb.PixelWidth;
            image.Height = imgThumb.PixelHeight;

            Canvas.SetLeft(image, 16);
            Canvas.SetTop(image, 16 + posCurrent);
            posCurrent += imgThumb.PixelHeight + 8;

            //this.canvasMain.Children.Add(image);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            using (var stream = new WebClient().OpenRead("http://pya.cc/ipn/index.php"))
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                var xml = ParseHtml(sr);

                XNamespace ns = "http://www.w3.org/1999/xhtml";
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
                        //this.rtboxMain.Text += strUrl + Environment.NewLine;
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
    }
}
