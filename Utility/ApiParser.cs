using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Verde.Utility
{
    [DataContract]
    public class TopInfo
    {
        [DataMember(Name = "kensu")]
        public int EntryCount { get; set; }
        [DataMember(Name = "view")]
        public int AcquisitionCount { get; set; }
        [DataMember(Name = "nowpage")]
        public int CurrentPage { get; set; }
        [DataMember(Name = "endpage")]
        public int EndPage { get; set; }
        [DataMember(Name = "sortid")]
        public string SortKey { get; set; }
        [DataMember(Name = "category")]
        public string Category { get; set; }
        [DataMember(Name = "imgdate")]
        public string Date { get; set; }
    }

    [DataContract]
    public class ShowcaseInfo
    {
        [DataMember(Name = "id")]
        public int EntryID { get; set; }
        [DataMember(Name = "genre")]
        public string Genre { get; set; }
        [DataMember(Name = "setdate")]
        public string EntryDate { get; set; }
        [DataMember(Name = "th")]
        public string Thumbnail { get; set; }
        [DataMember(Name = "th2")]
        public string Thumbnail2 { get; set; }
        [DataMember(Name = "img_cnt")]
        public int ImageSetCount { get; set; }
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "sex")]
        public int AdultCode { get; set; }
        [DataMember(Name = "ii")]
        public int GoodCount { get; set; }
        [DataMember(Name = "waru")]
        public int BadCount { get; set; }
        [DataMember(Name = "goukei")]
        public int RelativeCount { get; set; }
        [DataMember(Name = "kansou")]
        public string AuthorComment { get; set; }
        [DataMember(Name = "toukouname")]
        public string AuthorName { get; set; }
        [DataMember(Name = "via")]
        public string ViaString { get; set; }
        [DataMember(Name = "comecnt")]
        public int CommentCount { get; set; }
        [DataMember(Name = "kth1")]
        public string Commenter1Icon { get; set; }
        [DataMember(Name = "kname1")]
        public string Commenter1Name { get; set; }
        [DataMember(Name = "kdate1")]
        public string Commenter1Date { get; set; }
        [DataMember(Name = "kii1")]
        public string Commenter1Good { get; set; }
        [DataMember(Name = "kwaru1")]
        public string Commenter1Bad { get; set; }
        [DataMember(Name = "ksum1")]
        public string Commenter1Relative { get; set; }
        [DataMember(Name = "kcome1")]
        public string Commenter1Comment { get; set; }
        [DataMember(Name = "kth2")]
        public string Commenter2Icon { get; set; }
        [DataMember(Name = "kname2")]
        public string Commenter2Name { get; set; }
        [DataMember(Name = "kdate2")]
        public string Commenter2Date { get; set; }
        [DataMember(Name = "kii2")]
        public string Commenter2Good { get; set; }
        [DataMember(Name = "kwaru2")]
        public string Commenter2Bad { get; set; }
        [DataMember(Name = "ksum2")]
        public string Commenter2Relative { get; set; }
        [DataMember(Name = "kcome2")]
        public string Commenter2Comment { get; set; }
        [DataMember(Name = "kth3")]
        public string Commenter3Icon { get; set; }
        [DataMember(Name = "kname3")]
        public string Commenter3Name { get; set; }
        [DataMember(Name = "kdate3")]
        public string Commenter3Date { get; set; }
        [DataMember(Name = "kii3")]
        public string Commenter3Good { get; set; }
        [DataMember(Name = "kwaru3")]
        public string Commenter3Bad { get; set; }
        [DataMember(Name = "ksum3")]
        public string Commenter3Relative { get; set; }
        [DataMember(Name = "kcome3")]
        public string Commenter3Comment { get; set; }
    }
    
    class ApiParser
    {
        public static List<ShowcaseInfo> ParseIndexApi(string strUrl)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string str = client.DownloadString(strUrl);

            str = str.TrimStart('[').TrimEnd(']');
            string[] entries = str.Split('}');


            var strEntry = entries[0] + "}";
            var jsonBytes = Encoding.Unicode.GetBytes(strEntry);
            var sr = new MemoryStream(jsonBytes);
            var serializer = new DataContractJsonSerializer(typeof(TopInfo));
            var topInfo = serializer.ReadObject(sr) as TopInfo;

            var showcaseList = new List<ShowcaseInfo>();
            for (int i = 1; i <= topInfo.AcquisitionCount; i++) {
                strEntry = entries[i].TrimStart('\"').TrimStart(',') + "}";
                jsonBytes = Encoding.Unicode.GetBytes(strEntry);
                sr = new MemoryStream(jsonBytes);
                serializer = new DataContractJsonSerializer(typeof(ShowcaseInfo));
                showcaseList.Add(serializer.ReadObject(sr) as ShowcaseInfo);
            }

            System.Windows.MessageBox.Show(str);

            return showcaseList;
        }
    }
}
