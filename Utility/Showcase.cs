using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Verde.Utility
{
    class Showcase
    {
        public static string strArgForLarge = "&imgsize=1"; /* PCサイト版画像へのアクセス */
        public static string strImageID = "SAKUHIN_AREA";
        public static string strCommentID = "PYA_COMMENT_AREA";
        private string strBaseUrl;
        private static string[] arrIgnoreClasses = { "l_dot3", };

        public Showcase(string strBaseUrl)
        {
            this.strBaseUrl = strBaseUrl;
        }

        public void Import(string strUrl)
        {
            var strPageUrl = this.strBaseUrl + strUrl + Showcase.strArgForLarge;
            var xmlPage = HtmlParser.Parse(HtmlParser.OpenUrl(strPageUrl));

            foreach (var item in xmlPage.Descendants(HtmlParser.nsXhtml + "div")) {
                var attr = item.Attribute("id");
                if (attr != null && String.IsNullOrEmpty(attr.Value) == false) {
                    if (attr.Value.Equals(Showcase.strImageID)) {
                        List<string> listImages = this.GetImageUrl(item);
                    } else if (attr.Value.Equals(Showcase.strCommentID)) {
                        List<string> listComments = this.GetComments(item);
                    }
                }
            }
            return;
        }

        public List<string> GetImageUrl(XElement xmlImage)
        {
            foreach (var item in xmlImage.Descendants(HtmlParser.nsXhtml + "img")) {
                var attr = item.Attribute("src");
                if (attr != null && String.IsNullOrEmpty(attr.Value) == false) {
                    Console.WriteLine(attr.Value);
                }
            }
            return null;
        }

        public List<string> GetComments(XElement xmlComments)
        {
            foreach (var item in xmlComments.Descendants(HtmlParser.nsXhtml + "div")) {
                var attr = item.Attribute("class");
                if (attr != null) {
                    bool bIgnore = false;
                    foreach (var str in Showcase.arrIgnoreClasses) {
                        if (attr.Value.Equals(str)) {
                            bIgnore = true;
                            break;
                        }
                    }
                    if (bIgnore) continue;
                }

                //foreach (var node in item.Nodes()) {
                //    Console.WriteLine(node.ToString());
                //}

                Console.WriteLine(item.Value);
                //foreach (var node in item.Nodes()) {
                //    Console.WriteLine(node.ToString());
                //}
            }
            return null;
        }
    }
}
