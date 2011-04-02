using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

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
        private static bool bLargeThumb = true;

        public string UrlEntry { set; get; }
        public string UrlThumbnail { set; get; }
        public Int64 ImageID { set; get; }
        public Type ImageType { set; get; }
        public string Title { set; get; }
        public string Poster { set; get; }
        public int Good { set; get; }
        public int Bad { set; get; }
        public int Comment { set; get; }
        public int Pickup { set; get; }
        public List<int> Counts { set; get; }

        public Element()
        {
            this.Counts = new List<int>();
        }

        public void Import(XElement xmlElement)
        {
            int nCount = 0;
            foreach (var item in xmlElement.Descendants()) {
                if (item.Name.Equals(HtmlParser.nsXhtml + "a")) {
                    this.UrlEntry = item.Attribute("href").Value;
                    this.ImageID = Int64.Parse(this.UrlEntry.Substring(this.UrlEntry.LastIndexOf('=') + 1));
                } else if (item.Name.Equals(HtmlParser.nsXhtml + "img") && item.Attribute("class").Value == "thumb") {
                    string strUrl = item.Attribute("src").Value;
                    if (String.Compare("http:", 0, strUrl, 0, 5) != 0) {
                        if (Element.bLargeThumb == true) {
                            /* 現状決め打ち "/i"を除外する */
                            strUrl = strUrl.Substring(2);
                        }
                        //strUrl = "http://pya.cc" + strUrl;
                    }
                    this.UrlThumbnail = strUrl;
                    //this.dbImageCache.GetCache(strUrl, this.CheckPath);
                    //this.dbImageCache.GetImageCache(strUrl, this.DrawThumbImage);
                } else if (item.Name.Equals(HtmlParser.nsXhtml + "span")) {
                    XAttribute attr = item.Attribute("class");
                    if (attr != null && attr.Value == "block") {
                        switch (nCount++) {
                            case 0: this.ImageType = this.GetType(this.GetLastWord(item.Value)); break;
                            case 1: this.Title = this.GetInnerWord(item.Value, '「', '」'); break;
                            case 2: this.Poster = this.GetDelimitedWord(item.Value, ':'); break;
                            case 3: this.GetNumbers(item.Value, ':', this.Counts); break;
                        }
                    }
                }
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

        private string GetInnerWord(string strWord, char cBegin, char cEnd)
        {
            int nBegin = strWord.IndexOf(cBegin) + 1;
            int nEnd = strWord.IndexOf(cEnd);
            return strWord.Substring(nBegin, nEnd - nBegin);
        }

        private string GetLastWord(string strWords)
        {
            for (var i = 1; i < strWords.Length; i++) {
                string strRet = strWords.Substring(strWords.Length - i);
                if (Regex.IsMatch(strRet, @"^[a-zA-Z0-9]+$") == false) {
                    return strRet.Substring(1);
                }
            }
            return string.Empty;
        }

        private string GetDelimitedWord(string strWords, char cDelimiter)
        {
            return strWords.Substring(strWords.IndexOf(cDelimiter) + 1);
        }

        private void GetNumbers(string strWords, char cDelimiter, List<int> listNumbers)
        {
            string strTmp = strWords;
            for (;;) {
                var nPos = strTmp.IndexOf(cDelimiter);
                if (nPos < 0) break;
                string strNumber = strTmp.Substring(nPos + 1);
                for (var i = 0; strNumber.Length > 0; i++) {
                    if (Regex.IsMatch(strNumber, @"^[0-9]+$") == true) {
                        listNumbers.Add(int.Parse(strNumber));
                        strTmp = strTmp.Substring(strTmp.Length - i);
                        break;
                    }
                    strNumber = strNumber.Substring(0, strNumber.Length - 1);
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
