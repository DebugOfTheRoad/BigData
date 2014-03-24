using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net;
using System.Windows.Media.Imaging;
using System.IO;

namespace BigData.OCLC
{
    /// <summary>
    /// Manage access to the OCLC APIs.
    /// </summary>
    public class Client : PublicationSource
    {
        /// <summary>
        /// Create a new OCLCClient.
        /// </summary>
        /// <param name="key">The WSKey to use to access OCLC APIs</param>
        public Client(string key, string feedUri)
        {
            WSKey = key;
            FeedUri = feedUri;
        }

        /// <summary>
        /// Fetch publications from OCLC RSS
        /// </summary>
        /// <returns>An array of publications from the OCLC RSS API</returns>
        public async Task<IEnumerable<Publication>> GetPublications()
        {
            var doc = XDocument.Load(FeedUri);
            var tasks = from item in doc.Descendants("item")
                        let uri = new Uri(item.Element("link").Value)
                        let oclcNum = uri.Segments.Last()
                        select FetchPublicationFromOCLCNumber(oclcNum);

            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Web Services key needed to access OCLC APIs
        /// </summary>
        public string WSKey { get; set; }

        /// <summary>
        /// The RSS feed URI from which to fetch publications
        /// </summary>
        public string FeedUri { get; set; }

        private async Task<Publication> FetchPublicationFromOCLCNumber(string oclcNumber)
        {
            var baseUri = @"http://www.worldcat.org/webservices/catalog/content/";
            var queryURI = baseUri + oclcNumber + "?wskey=" + WSKey;

            var request = WebRequest.Create(queryURI);
            var response = await request.GetResponseAsync();

            var doc = XDocument.Load(response.GetResponseStream());
            var pub = Publication.FromXML(doc);
            pub.OCLCNumber = oclcNumber;

            var allISBNs = await FetchRelatedISBNs(pub.ISBNs);
            pub.CoverImage = CoverImageForISBNs(allISBNs);

            return pub;
        }

        private async Task<string[]> FetchRelatedISBNs(IEnumerable<string> isbns)
        {
            var baseUri = new Uri(@"http://xisbn.worldcat.org/webservices/xid/isbn/");

            var requestUri = new Uri(baseUri, isbns.First() + @"?method=getEditions&format=xml");

            var request = WebRequest.Create(requestUri);
            XNamespace ns = @"http://worldcat.org/xid/isbn/";
            try
            {
                var response = await request.GetResponseAsync();
                var doc = XDocument.Load(response.GetResponseStream());
                return (from tag in doc.Descendants(ns + "isbn")
                        select tag.Value)
                       .Concat(isbns)
                       .Distinct()
                       .ToArray();
            }
            catch (WebException)
            {
                return isbns.ToArray();
            }
        }

        private BitmapImage CoverImageForISBNs(string[] isbns)
        {
            var baseUri = new Uri(@"http://covers.openlibrary.org/b/isbn/");
            var requestUris = (from isbn in isbns
                              select new Uri(baseUri, isbn + @"-L.jpg?default=false"))
                              .ToList();

            // make sure there is always one good URI
            requestUris.Add(new Uri(@"http://placehold.it/250x400"));

            foreach (var uri in requestUris)
            {
                var request = WebRequest.Create(uri);
                try
                {
                    var response = request.GetResponse();

                    var ms = new MemoryStream();
                    response.GetResponseStream().CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();

                    if (image.PixelHeight >= 300) return image;
                }
                catch (WebException) { }
            }

            throw new Exception("No cover images");
        }
    }
}
