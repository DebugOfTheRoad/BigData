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
            List<Publication> toRet = new List<Publication>();
            foreach (String number in oclcNumbers)
            {
                String OCLCQueryURL = "http://www.worldcat.org/webservices/catalog/content/" + number + "?wskey=" + this.WSKey;
                XmlDocument xmldoc = new System.Xml.XmlDocument();
                xmldoc.Load(OCLCQueryURL);
                XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmldoc.NameTable);
                xmlnsManager.AddNamespace("default", "http://www.loc.gov/MARC21/slim");
                XmlNode isbnNode = xmldoc.SelectSingleNode("//default:datafield[@tag='020']", xmlnsManager);
                String isbn, description, title;
                if (isbnNode == null)
                {
                    isbn = "";
                }
                else
                {
                    String[] arr = isbnNode.InnerText.Split(' '); // getting rid of extraneous text
                    isbn = arr[0];
                }
                XmlNode descNode = xmldoc.SelectSingleNode("//default:datafield[@tag='520']", xmlnsManager);
                if (descNode == null)
                {
                    description = "";
                }
                else
                {
                    description = descNode.InnerText;
                }
                XmlNode titleNode = xmldoc.SelectSingleNode("//default:datafield[@tag='245']", xmlnsManager);
                if (titleNode == null)
                {
                    title = "";
                }
                else
                {
                   title = titleNode.InnerText.Split('[')[0];

                }

                Publication toAdd = new Publication();
                toAdd.oclcNumber = number;
                toAdd.isbn = isbn;
                toAdd.coverImage = getCover(isbn);
                toAdd.title =  title;
                toAdd.desc = description;
                toRet.Add(toAdd);
                Console.WriteLine(toAdd.printBook());
            }
            return toRet;
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
                    return webImage;
                }
                catch (WebException e) { }
            }
            Console.WriteLine("failed to get book cover");
            return null;

        }

        // Returns a list of related isbns using the xisbn api
        private List<String> getRelatedISBNs(String isbn)
        {
            List<String> toRet = new List<String>();
            String xISBNQuery = "http://xisbn.worldcat.org/webservices/xid/isbn/" + isbn + "?method=getEditions&format=xml";

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
