using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using Sgml;
using System.Net;

namespace Verde
{
    public partial class GlobalForm : Form
    {
        public GlobalForm()
        {
            InitializeComponent();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            using (var stream = new WebClient().OpenRead("http://pya.cc/ipn/index.php"))
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                var xml = ParseHtml(sr);
                foreach (var item in xml.Descendants("h3"))
                {
                    this.rtboxMain.Text += item.Value;
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
