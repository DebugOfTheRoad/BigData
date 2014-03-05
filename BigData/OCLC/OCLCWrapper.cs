using System;
using System.Xml;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Drawing;

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
            foreach (String number in oclcNumbers)
            {
                String OCLCQueryURL = "http://www.worldcat.org/webservices/catalog/content/" + number + "?wskey=" + this.WSKey;
                WebClient client = new WebClient();
                string downloadString = client.DownloadString(OCLCQueryURL);
                var array = downloadString.Split('\n');
                String isbn = "";
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Contains("tag=\"020\""))
                    {
                        var arr = (array[i+1]).Split('>');
                        arr = arr[1].Split(' ');
                        arr = arr[0].Split('<');
                        isbn = arr[0];
                        break;
                    }
                }
                Image coverImage = getCover(isbn);
                Publication toAdd = new Publication();
                toAdd.oclcNumber = number;
                toAdd.isbn = isbn;
                toAdd.coverImage = coverImage;
            }
                 
            return new List<Publication>();
        }

        private Image getCover(String isbn)
        {
            List<String> relatedISBNs = getRelatedISBNs(isbn);
            relatedISBNs.Insert(0, isbn);
            String coverURL;
            WebClient client = new WebClient();
            WebRequest requestPic;
            foreach (String related in relatedISBNs)
            {
                coverURL = "http://covers.openlibrary.org/b/isbn/" + related + "-L.jpg?default=false";
                requestPic = WebRequest.Create(coverURL);
                try
                {
                    WebResponse responsePic = requestPic.GetResponse();
                    Image webImage = Image.FromStream(responsePic.GetResponseStream());
                    Console.WriteLine("hey we got a book cover!");
                    return webImage;
                }
                catch (WebException e)
                {
                    //Console.WriteLine("well, we didn't get one");
                }
            }

            return null;
            
        }

        // Returns a list of related isbns using the xisbn api
        private List<String> getRelatedISBNs(String isbn)
        {
            List<String> toRet = new List<String>();
            String xISBNQuery = "http://xisbn.worldcat.org/webservices/xid/isbn/" + isbn  + "?method=getEditions&format=xml";

            XmlDocument xmldoc = new System.Xml.XmlDocument();
            xmldoc.Load(xISBNQuery);
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmldoc.NameTable);
            xmlnsManager.AddNamespace("default", "http://worldcat.org/xid/isbn/");


            XmlNodeList isbnlist;

            isbnlist = xmldoc.SelectNodes("//default:isbn", xmlnsManager);

            foreach (XmlNode currentISBN in isbnlist)
            {
                toRet.Add(currentISBN.InnerText);
            }
            return toRet;

        }

    
    }

}
