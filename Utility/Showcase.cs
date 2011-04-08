using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Verde.Utility
{
    class Showcase
    {
        private string strBaseUrl;

        public Showcase(string strBaseUrl)
        {
            this.strBaseUrl = strBaseUrl;
        }

        public void Import(string strUrl)
        {
            string strPageUrl = this.strBaseUrl + strUrl;
            XDocument xmlPage = HtmlParser.Parse(HtmlParser.OpenUrl(strPageUrl));

            foreach (var item in xmlPage.Descendants(HtmlParser.nsXhtml + "li")) {
                Console.WriteLine(item.ToString());
            }
            return;
        }
    }
}
