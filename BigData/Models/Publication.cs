using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;


namespace BigData {
    public class Publication {

        public static Publication FromXML(XDocument doc) {
            var pub = new Publication();
            pub.Title = GetOCLCFieldByTag(titleTag, doc);
            pub.Description = GetOCLCFieldByTag(descTag, doc);
            pub.ISBNs = GetOCLCFieldsByTag(isbnTag, doc);

            var firstAuthor = GetOCLCFieldByTag(authorTag, doc);
            var allAuthors = GetOCLCFieldsByTag(authorsTag, doc);
            allAuthors.Insert(0, firstAuthor);
            pub.Authors = allAuthors;


            return pub;
        }

        public override string ToString() {
            return String.Format("BigData.Publication<Title: {0}, ISBN: {1}>", this.Title, this.ISBNs.First());
        }

        public string Title {
            get { return title; }
            set {
                var ti = new System.Globalization.CultureInfo("en-US").TextInfo;
                title = ti.ToTitleCase(value);
            }
        }

        public string OCLCNumber { get; set; }
        public string Description { get; set; }
        public List<string> Authors { get; set; }
        public string CoverImageURI { get; set; }

        public BitmapImage CoverImage { get; set; }

        public List<string> ISBNs {
            get { return isbns; }
            set {
                isbns = (from isbn in value
                         where isbn != null
                         select isbn.Split(new char[] { ' ' }, 2).First())
                         .ToList();
            }
        }

        private static string GetOCLCFieldByTag(string tag, XDocument doc) {
            XNamespace ns = @"http://www.loc.gov/MARC21/slim";
            try {
                return (from datafield in doc.Descendants(ns + "datafield")
                        where datafield.Attribute("tag").Value.Equals(tag)
                        select datafield.Descendants().First().Value)
                        .First();
            } catch (InvalidOperationException) {
                return null;
            }
        }

        private static List<string> GetOCLCFieldsByTag(string tag, XDocument doc) {
            XNamespace ns = @"http://www.loc.gov/MARC21/slim";
            return (from datafield in doc.Descendants(ns + "datafield")
                    where datafield.Attribute("tag").Value.Equals(tag)
                    select datafield.Descendants().First().Value)
                    .ToList();
        }

        private List<string> isbns;
        private string title;

        private const string formTag = "655";
        private const string authorTag = "100";
        private const string authorsTag = "700";
        private const string isbnTag = "020";
        private const string descTag = "520";
        private const string contentsTag = "505";
        private const string titleTag = "245";
    }
}
