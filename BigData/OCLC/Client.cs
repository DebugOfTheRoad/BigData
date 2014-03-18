using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net;
using System.Windows.Media.Imaging;

namespace BigData.OCLC
{
    /// <summary>
    /// Manage access to the OCLC APIs.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Create a new OCLCClient.
        /// </summary>
        /// <param name="key">The WSKey to use to access OCLC APIs</param>
        public Client(string key)
        {
            WSKey = key;
        }

        /// <summary>
        /// Return a list of publications described by the RSS document at <c>feedUri</c>.
        /// </summary>
        /// <param name="feedUri"></param>
        /// <returns>A list of publications</returns>
        public IEnumerable<Publication> FetchPublicationsFromRSS(string feedUri)
        {
            var doc = XDocument.Load(feedUri);
            return from item in doc.Descendants("item")
                   let uri = new Uri(item.Element("link").Value)
                   let oclcNum = uri.Segments.Last()
                   select FetchPublicationFromOCLCNumber(oclcNum);
        }

        /// <summary>
        /// Web Services key needed to access OCLC APIs
        /// </summary>
        public string WSKey { get; set; }

        private Publication FetchPublicationFromOCLCNumber(string oclcNumber)
        {
            var baseUri = @"http://www.worldcat.org/webservices/catalog/content/";
            var queryURI = baseUri + oclcNumber + "?wskey=" + WSKey;
            var doc = XDocument.Load(queryURI);

            var pub = Publication.FromXML(doc);
            pub.OCLCNumber = oclcNumber;
            pub.CoverImage = CoverImageForISBNs(FetchRelatedISBNs(pub.ISBNs.First()));

            return pub;
        }

        private IEnumerable<string> FetchRelatedISBNs(string isbn)
        {
            var baseUri = @"http://xisbn.worldcat.org/webservices/xid/isbn/";
            var queryUri = baseUri + isbn + @"?method=getEditions&format=xml";

            var doc = XDocument.Load(queryUri);
            XNamespace ns = @"http://worldcat.org/xid/isbn/";

            return from tag in doc.Descendants(ns + "isbn")
                   select tag.Value;
        }

        private BitmapImage CoverImageForISBNs(IEnumerable<string> isbns)
        {
            foreach (var isbn in isbns)
            {
                var queryString = @"http://covers.openlibrary.org/b/isbn/" + isbn + @"-L.jpg?default=false";
                var request = WebRequest.Create(queryString);

                try
                {
                    var response = request.GetResponse();
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = response.GetResponseStream();
                    image.EndInit();

                    Console.WriteLine("Got image for ISBN " + isbn);
                    return image;
                }
                catch (WebException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return null;
        }
    }
}
