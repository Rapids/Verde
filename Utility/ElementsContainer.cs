using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media;
using System.Windows.Documents;

namespace Verde.Utility
{
    class Element
    {
        public enum Type
        {
            UNKNOWN,
            PHOTO,
            VIDEO,
            TEXT,
            FLASH,
            EROINA
        }

        public static string strKeyword = "pyaimg-list";
        public static string strBaseUrl = "http://pya.cc";
        private static bool bLargeThumb = true;

        private string strUrlEntry;
        public string UrlEntry { get { return this.strUrlEntry;  } }

        private string strUrlThumbnail;
        public string UrlThumbnail { get { return this.strUrlThumbnail; } }

        private Int64 nImageID;
        public Int64 ImageID { get { return this.nImageID; } }

        private Type typeImage;
        public Type ImageType { get { return this.typeImage; } }

        private string strTitle;
        public string Title { get { return this.strTitle; } }

        private string strPoster;
        public string Poster { get { return this.strPoster; } }

        private List<int> listCounts;
        public List<int> Counts { get { return this.listCounts; } }

        private Showcase scContent;
        public Showcase Content { get { return this.scContent; } }

        public Element()
        {
            this.listCounts = new List<int>();
        }

        public void Import(XElement xmlElement)
        {
            int nCount = 0;
            foreach (var item in xmlElement.Descendants()) {
                if (item.Name.Equals(HtmlParser.nsXhtml + "a")) {
                    this.strUrlEntry = item.Attribute("href").Value;
                    this.nImageID = Int64.Parse(this.UrlEntry.Substring(this.UrlEntry.LastIndexOf('=') + 1));
                } else if (item.Name.Equals(HtmlParser.nsXhtml + "img") && item.Attribute("class").Value == "thumb") {
                    string strUrl = item.Attribute("src").Value;
                    if (String.Compare("http:", 0, strUrl, 0, 5) != 0) {
                        if (Element.bLargeThumb == true) {
                            strUrl = strUrl.Substring(2); /* 現状決め打ち "/i"を除外する */
                        }
                    }
                    this.strUrlThumbnail = strUrl;
                } else if (item.Name.Equals(HtmlParser.nsXhtml + "span")) {
                    XAttribute attr = item.Attribute("class");
                    if (attr != null && attr.Value == "block") {
                        switch (nCount++) {
                            case 0: this.typeImage = this.GetType(StringProcessing.GetLastWord(item.Value)); break;
                            case 1: this.strTitle = StringProcessing.GetInnerWord(item.Value, '「', '」'); break;
                            case 2: this.strPoster = StringProcessing.GetDelimitedWord(item.Value, ':'); break;
                            case 3: StringProcessing.GetNumbers(item.Value, ':', this.Counts); break;
                        }
                    }
                }
                this.scContent = new Showcase(Element.strBaseUrl);
                this.scContent.Import(this.UrlEntry);
            }
        }

        Type GetType(string strType)
        {
            Type typeRet = Type.UNKNOWN;

            if (strType == "Photo") {
                typeRet = Type.PHOTO;
            } else if (strType == "Video") {
                typeRet = Type.VIDEO;
            } else if (strType == "Text") {
                typeRet = Type.TEXT;
            } else if (strType == "Flash") {
                typeRet = Type.FLASH;
            } else if (strType == "Eroina") {
                typeRet = Type.EROINA;
            }

            return typeRet;
        }

        //private void CheckPath(string strPath)
        //{
        //    //Console.WriteLine(strPath);
        //}

        //private void SetThumbImage(BitmapImage imgThumb)
        //{
        //}

        public Paragraph MakeParagraph()
        {
            Paragraph p = new Paragraph();
            p.Inlines.Add("Title : ");
            var span = new Span { Foreground = Brushes.BlueViolet };
            span.Inlines.Add(this.Title);
            p.Inlines.Add(span);
            p.Inlines.Add("\n");

            p.Inlines.Add("Poster : ");
            span = new Span { Foreground = Brushes.Brown };
            span.Inlines.Add(this.Poster);
            p.Inlines.Add(span);
            p.Inlines.Add("\n");

            p.Inlines.Add("imgid : ");
            span = new Span { Foreground = Brushes.BurlyWood };
            span.Inlines.Add(string.Format("{0}", this.ImageID));
            p.Inlines.Add(span);
            p.Inlines.Add("\n");

            string strUrl = "http://pya.cc" + this.UrlEntry;
            p.Inlines.Add("URL : ");
            span = new Span { Foreground = Brushes.BurlyWood };
            var link = new Hyperlink();
            link.Inlines.Add(strUrl);
            link.NavigateUri = new Uri(strUrl);
            link.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(link_MouseLeftButtonDown);
            span.Inlines.Add(link);
            p.Inlines.Add(span);
            p.Inlines.Add("\n");

            if (this.Counts.Count > 0) {
                p.Inlines.Add("Counts : ");
                span = new Span { Foreground = Brushes.CadetBlue };
                span.Inlines.Add(string.Format("G:{0}, B:{1}, C:{2}", this.Counts[0], this.Counts[1], this.Counts[2]));
                p.Inlines.Add(span);
                //p.Inlines.Add("\n");
            }

            return p;
        }

        void link_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }

    class ElementsPack // contains elements on a page
    {
        private List<Element> listElements;
        public List<Element> Elements { get { return this.listElements; } }

        public ElementsPack()
        {
            this.listElements = new List<Element>();
        }

        public void Import(XDocument xmlHome)
        {
            // Extract Elements
            foreach (var item in xmlHome.Descendants(HtmlParser.nsXhtml + "li")) {
                XAttribute attr = item.Attribute("class");
                string[] arrValue = attr.Value.Split(' ');

                if (arrValue[0].Equals(Element.strKeyword)) {
                    Element element = new Element();
                    element.Import(item);
                    this.listElements.Add(element);
                }
            }
        }
    }

    class ElementsContainer
    {
        private Dictionary<int, ElementsPack> listElementsPack;
        private List<Element> listAllElements;
        public List<Element> Elements { get { return this.listAllElements; } }
        private string strBaseUrl;

        private bool bSetBackgroundColor = false;
        public Color BackgroundColor { set; get; }

        public ElementsContainer(string strBaseUrl)
        {
            this.listElementsPack = new Dictionary<int, ElementsPack>();
            this.listAllElements = new List<Element>();
            this.strBaseUrl = strBaseUrl;
        }

        public void ImportPage(int nPage)
        {
            string strPageUrl = this.strBaseUrl + nPage.ToString();
            XDocument xmlPage = HtmlParser.Parse(HtmlParser.OpenUrl(strPageUrl));
            ElementsPack epNew = new ElementsPack();

            epNew.Import(xmlPage);
            this.listElementsPack.Add(nPage, epNew);
            this.listAllElements.AddRange(epNew.Elements);

            this.CheckBackgroundColor(xmlPage);
        }

        public void ImportPages(int nPageMin, int nPageMax)
        {
            for (var i = nPageMin; i <= nPageMax; i++) {
                this.ImportPage(i);
            }
        }

        private void CheckBackgroundColor(XDocument xmlPage)
        {
            if (this.bSetBackgroundColor) return;

            // Check Background Color from style
            string strStyle = xmlPage.Root.Element(HtmlParser.nsXhtml + "body").Attribute("style").Value;
            string strBackground = "background-color:#";
            if (String.Compare(strBackground, 0, strStyle, 0, strBackground.Length, true) == 0) {
                string strBgValue = strStyle.Substring(strBackground.Length, strStyle.Length - strBackground.Length);
                if (strBgValue.Length >= 6) {
                    strBgValue = strBgValue.Substring(0, 6);
                    if (StringProcessing.IsHexadecimal(strBgValue) == true) {
                        this.BackgroundColor = StringProcessing.ConvertFromHexStringToColor(strBgValue);
                        this.bSetBackgroundColor = true;
                    }
                }
            }
        }
    }
}
