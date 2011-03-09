using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Net;
using Sgml;

namespace Verde.Utility
{
    class HtmlParser
    {
        public static XNamespace nsXhtml = "http://www.w3.org/1999/xhtml";

        public static StreamReader OpenUrl(string strUrl)
        {
            StreamReader stmRet = null;
            try {
                stmRet = new StreamReader(new WebClient().OpenRead(strUrl), Encoding.UTF8);
            } catch {
                return null;
            }
            return stmRet;
        }

        public static XDocument Parse(TextReader reader)
        {
            using (var sgmlReader = new SgmlReader { DocType = "HTML", CaseFolding = CaseFolding.ToLower }) {
                sgmlReader.InputStream = reader;
                return XDocument.Load(sgmlReader);
            }
        }
    }
}
