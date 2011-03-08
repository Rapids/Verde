using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Sgml;

namespace Verde.Utility {
    class HtmlParser {
        public static XDocument Parse(TextReader reader) {
            using (var sgmlReader = new SgmlReader { DocType = "HTML", CaseFolding = CaseFolding.ToLower }) {
                sgmlReader.InputStream = reader;
                return XDocument.Load(sgmlReader);
            }
        }
    }
}
