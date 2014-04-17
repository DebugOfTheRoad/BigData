using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Net;
using System.Windows.Media.Imaging;
using System.IO;
using HtmlAgilityPack;

namespace BigData.OCLC {
    /// <summary>
    /// Manage access to the OCLC APIs.
    /// </summary>
    public class Client : PublicationSource {

        /// <summary>
        /// Create a new OCLCClient.
        /// </summary>
        /// <param name="key">The WSKey to use to access OCLC APIs</param>
        /// <param name="feedUri">The RSS feed from which to fetch books</param>
        public Client(string key, string feedUri) {
            WSKey = key;
            FeedUri = feedUri;
        }

        /// <summary>
        /// Fetch publications from OCLC RSS
        /// </summary>
        /// <returns>An array of publications from the OCLC RSS API</returns>
        public async Task<IEnumerable<Publication>> GetPublications() {
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

        private async Task<Publication> FetchPublicationFromOCLCNumber(string oclcNumber) {
            var baseUri = @"http://www.worldcat.org/webservices/catalog/content/";
            var queryURI = baseUri + oclcNumber + "?wskey=" + WSKey;

            var request = WebRequest.Create(queryURI);
            var response = await request.GetResponseAsync();

            var doc = XDocument.Load(response.GetResponseStream());
            var pub = Publication.FromXML(doc);
            pub.OCLCNumber = oclcNumber;

            var coverUri = await GetOCLCCoverImageUriAsync(oclcNumber);
            pub.CoverImage = await GetBitmapImage(coverUri);

            return pub;
        }

        private async static Task<Uri> GetOCLCCoverImageUriAsync(string oclcNumber) {
            var baseUri = new Uri(@"https://bucknell.worldcat.org/oclc/");
            var oclcUri = new Uri(baseUri, oclcNumber);
            var request = WebRequest.CreateHttp(oclcUri);
            var response = await request.GetResponseAsync();

            var doc = new HtmlDocument();
            doc.Load(response.GetResponseStream());
            var img = doc.DocumentNode.SelectSingleNode(@"//*[@id='cover']/img");

            var src = img.Attributes["src"].Value.Replace("_140.jpg", "_400.jpg");
            return new Uri(baseUri.Scheme + ":" + src);
        }

        private static async Task<BitmapImage> GetBitmapImage(Uri imageUri) {
            var request = WebRequest.CreateHttp(imageUri);
            var response = await request.GetResponseAsync();

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = response.GetResponseStream();
            image.EndInit();

            return image;
        }
    }
}
