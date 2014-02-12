using System;
using System.Xml;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BigData
{

    public class OCLCWrapper
    {
        string WSKey;
        XmlTextReader reader;
        public OCLCWrapper(string rssURL, string key, string secretKey )
        {
            reader = new XmlTextReader(rssURL);
            this.WSKey = key;
        }

        public List<Publication> createPublications()
        {
            List<Publication> publications = new List<Publication>();
            var xdoc = new XDocument();
            try
            {
                xdoc = XDocument.Load(reader);

            }
            catch (System.Net.WebException e)
            {
                System.Console.WriteLine("could not connect to oclc");
                System.Console.WriteLine(e.Message);
            }
            string[] oclcNumbers = getOCLCNumbers(xdoc);
            foreach (String number in oclcNumbers) {
                publications.Add(new Publication(number));
            }
            queryOCLC(oclcNumbers); // assign publications when done
            return publications;
        }

        /*
         * Parses the OCLC numbers out of the RSS xml
         */
        private  string[] getOCLCNumbers(XDocument xdoc)
        {
            var q = from b in xdoc.Descendants("item")
                    select new
                    {
                        name = b.Element("guid").Value,
                    };
            var r = q.ToArray();
            string[] toRet = new string[r.Length];
            for (int i = 0; i<r.Length; i++) {
                string[] temp = r[i].ToString().Split('/', ' ');
                toRet[i] = temp[temp.Length - 2];
            }
            return toRet;
        }

        private List<Publication> queryOCLC(string[] oclcNumbers)
        {
            XmlReader pubReader;
            foreach (String number in oclcNumbers)
            {
                String OCLCQueryURL = "http://www.worldcat.org/webservices/catalog/content/" + number + "?wskey=" + this.WSKey;
                pubReader = new XmlTextReader(OCLCQueryURL);
                Console.WriteLine(pubReader.ToString());
                String coverURL = "http://covers.openlibrary.org/b/oclc/" + number + "-L.jpg"; //problem: get isbn of normal book?
                Console.WriteLine(coverURL);
            }
            return new List<Publication>();
        }
    }

}
