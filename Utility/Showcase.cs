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
        public static string strCommentClass = "cmt-1";
        public static string strResID = "RES_CMT_";
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
                    Logger.GlobalLogger.Write(attr.Value);
                }
            }
        }

        public void MakeComments(XElement xmlComments)
        {
            var i = 0;
            Comment comment = null;
            foreach (var item in xmlComments.Descendants(HtmlParser.nsXhtml + "div")) {
                var attrClass = item.Attribute("class");
                if (attrClass != null) {
                    bool bIgnore = false;
                    foreach (var str in Showcase.arrIgnoreClasses) {
                        if (attrClass.Value.Equals(str)) {
                            bIgnore = true;
                            break;
                        }
                    }
                    if (bIgnore) continue;
                    if (attrClass.Value.Equals(Showcase.strCommentClass)) {
                        comment.Word = item.Value;
                        continue;
                    }
                } else {
                    var attrID = item.Attribute("id");
                    if (attrID != null && attrID.Value.StartsWith(Showcase.strResID)) {
                        Logger.GlobalLogger.Write(attrID.Value);
                        continue;
                    }
                }

                var itemSpan = this.ExtractTaggedField(item, "span");
                var itemTable = this.ExtractTaggedField(item, "table");

                if (itemSpan == null && itemTable == null) {
                    if (comment != null) {
                        this.listComments.Add(comment.Number, comment);
                    }
                    comment = this.CreateComment(item);
                } else if (itemSpan != null) {
                }
            }

            if (comment != null) {
                this.listComments.Add(comment.Number, comment);
            }
        }

        private XElement ExtractTaggedField(XElement element, string strTag)
        {
            IEnumerable<XElement> items = element.Descendants(HtmlParser.nsXhtml + strTag);
            if (items != null && items.Count() > 0) {
                 return items.First();
            }
            return null;
        }

        private Comment CreateComment(XElement xmlUser)
        {
            string strNumber = xmlUser.FirstNode.ToString();
            string strUser = xmlUser.LastNode.ToString();
            //string strUser = xmlUser.FirstNode.NextNode.NextNode.ToString();

            var itemAhref = xmlUser.Descendants(HtmlParser.nsXhtml + "img").First();
            string strIconUrl = itemAhref.Attribute("src").Value;
            string strTitle = itemAhref.Attribute("title").Value;
            List<int> listCounts = new List<int>();
            StringProcessing.GetNumbers(strTitle, '=', listCounts);

            Comment comment = new Comment();
            comment.Number = int.Parse(strNumber.Substring(0, strNumber.IndexOf('.')));
            comment.User = UserManager.GlobalUserManager.Register(strUser, strIconUrl);
            comment.User.WriteCount = listCounts[0];
            comment.User.PickedupCount = listCounts[1];

            return comment;
        }
    }
}
