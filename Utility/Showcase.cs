using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Verde.Utility
{
    class Comment
    {
        public int Number { set; get; }
        public User User { set; get; }
        public string Word { set; get; }

        public Comment()
        {
        }
    }

    class Showcase
    {
        public static string strArgForLarge = "&imgsize=1"; /* PCサイト版画像へのアクセス */
        public static string strImageID = "SAKUHIN_AREA";
        public static string strCommentID = "PYA_COMMENT_AREA";
        private string strBaseUrl;
        private List<string> listImages;
        private Dictionary<int, Comment> listComments;
        private static string[] arrIgnoreClasses = { "l_dot3", };

        private bool bCompleted = false;
        public bool Completed { get { return this.bCompleted; } }

        public Showcase(string strBaseUrl)
        {
            this.strBaseUrl = strBaseUrl;
            this.listImages = new List<string>();
            this.listComments = new Dictionary<int, Comment>();
        }

        public void Import(string strUrl)
        {
            var strPageUrl = this.strBaseUrl + strUrl + Showcase.strArgForLarge;
            var xmlPage = HtmlParser.Parse(HtmlParser.OpenUrl(strPageUrl));

            foreach (var item in xmlPage.Descendants(HtmlParser.nsXhtml + "div")) {
                var attr = item.Attribute("id");
                if (attr != null && String.IsNullOrEmpty(attr.Value) == false) {
                    if (attr.Value.Equals(Showcase.strImageID)) {
                        this.MakeImageUrl(item);
                    } else if (attr.Value.Equals(Showcase.strCommentID)) {
                        this.MakeComments(item);
                    }
                }
            }
            this.bCompleted = true;
        }

        public void MakeImageUrl(XElement xmlImage)
        {
            foreach (var item in xmlImage.Descendants(HtmlParser.nsXhtml + "img")) {
                var attr = item.Attribute("src");
                if (attr != null && String.IsNullOrEmpty(attr.Value) == false) {
                    this.listImages.Add(attr.Value);
                    //Console.WriteLine(attr.Value);
                }
            }
        }

        public void MakeComments(XElement xmlComments)
        {
            var i = 0;
            Comment comment = null;
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

                switch (i++) {
                    case 0:
                        comment = new Comment();
                        comment.Number = int.Parse(item.Value.Substring(0, item.Value.IndexOf('.')));
                        comment.User = new User(item.FirstNode.NextNode.NextNode.ToString());
                        break;
                    case 1: break;
                    case 2: break;
                    case 3: break;
                    case 4:
                        this.listComments.Add(comment.Number, comment);
                        i = 0;
                        break;
                }

                //Console.WriteLine(item.Value);
                //foreach (var node in item.Nodes()) {
                //    Console.WriteLine(node.ToString());
                //}
            }
        }
    }
}
