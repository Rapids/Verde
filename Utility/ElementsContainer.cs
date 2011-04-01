using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Verde.Utility
{
    class Element
    {
        public static string strKeyword = "pyaimg-list";
        private static bool bLargeThumb = true;

        public string UrlEntry { set; get; }
        public string UrlThumbnail { set; get; }
        public Int64 Number { set; get; }
        public string Type { set; get; }
        public string Title { set; get; }
        public string Poster { set; get; }
        public int Good { set; get; }
        public int Bad { set; get; }
        public int Comment { set; get; }

        public Element()
        {
        }

        public void Import(XElement xmlElement)
        {
            var link = xmlElement.Element(HtmlParser.nsXhtml + "a");
            var image = link.Element(HtmlParser.nsXhtml + "img");

            int nCount = 0;
            foreach (var item in xmlElement.Descendants()) {
                if (item.Name.Equals(HtmlParser.nsXhtml + "a")) {
                    this.UrlEntry = item.Attribute("href").Value;
                } else if (item.Name.Equals(HtmlParser.nsXhtml + "img")) {
                    if (item.Attribute("class").Value == "thumb") {
                        string strUrl = item.Attribute("src").Value;
                        if (String.Compare("http:", 0, strUrl, 0, 5) != 0) {
                            if (Element.bLargeThumb == true) {
                                /* 現状決め打ち "/i"を除外する */
                                strUrl = strUrl.Substring(2);
                            }
                            strUrl = "http://pya.cc" + strUrl;
                        }
                        this.UrlThumbnail = strUrl;
                        //this.dbImageCache.GetCache(strUrl, this.CheckPath);
                        //this.dbImageCache.GetImageCache(strUrl, this.DrawThumbImage);
                    }
                } else if (item.Name.Equals(HtmlParser.nsXhtml + "span")) {
                    switch (nCount++) {
                        case 0: this.Type = item.Value; break;
                        case 1: this.Title = item.Value; break;
                        case 2: this.Poster = item.Value; break;
                        //case 3: this.Type = item.Value; break;
                    }
                }
            }
        }
    }

    class ElementsPack // contains elements on a page
    {
        private List<Element> listElements;

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
        private string strBaseUrl;

        public ElementsContainer(string strBaseUrl)
        {
            this.listElementsPack = new Dictionary<int, ElementsPack>();
            this.listAllElements = new List<Element>();
            this.strBaseUrl = strBaseUrl;
        }

        public void ImportPage(int nPage)
        {
            string strPageUrl = strBaseUrl + nPage.ToString();
            XDocument xmlPage = HtmlParser.Parse(HtmlParser.OpenUrl(strPageUrl));
            ElementsPack epNew = new ElementsPack();

            epNew.Import(xmlPage);
            this.listElementsPack.Add(nPage, epNew);
        }

        public void ImportPages(int nPageMin, int nPageMax)
        {
            for (var i = nPageMin; i <= nPageMax; i++) {
                this.ImportPage(i);
            }
        }
    }
}
