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
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

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

        /// <summary>
        /// Populates a publication object from an OCLC number
        /// </summary>
        /// <param name="oclcNumber">The OCLC number representing the material</param>
        /// <returns>A publication object</returns>
        private async Task<Publication> FetchPublicationFromOCLCNumber(string oclcNumber) {
            var baseUri = @"http://www.worldcat.org/webservices/catalog/content/";
            var queryURI = baseUri + oclcNumber + "?wskey=" + WSKey;

            var request = WebRequest.Create(queryURI);
            var response = await request.GetResponseAsync();

            var doc = XDocument.Load(response.GetResponseStream());
            var pub = Publication.FromXML(doc);
            pub.OCLCNumber = oclcNumber;

            try {
                var imageUriTasks = from num in await FetchAllOCLCNumbers(oclcNumber)
                                    select GetOCLCCoverImageUriAsync(oclcNumber);

                var uris = (await Task.WhenAll(imageUriTasks)).SelectMany(i => i);
                var imageTasks = from uri in uris
                                 select GetBitmapImage(uri);

                var goodImages = from source in await Task.WhenAll(imageTasks)
                                 where source != null
                                 select source;

                pub.CoverImage = goodImages.First();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                pub.CoverImage = DrawPublicationImage(pub.Title, String.Join(", ", pub.Authors));
            }

            return pub;
        }

        public static async Task<IEnumerable<string>> FetchAllOCLCNumbers(string oclcNumber) {
            var baseUri = new Uri(@"http://xisbn.worldcat.org/webservices/xid/oclcnum/");
            var queryUri = new Uri(baseUri, oclcNumber + "?method=getEditions&format=xml&fl=oclcnum");

            var request = WebRequest.CreateHttp(queryUri);
            var response = await request.GetResponseAsync();

            var doc = XDocument.Load(response.GetResponseStream());
            XNamespace ns = @"http://worldcat.org/xid/oclcnum/";
            return from tag in doc.Descendants(ns + "oclcnum")
                   select tag.Value;
        }

        /// <summary>
        /// Returns the URI of the best available cover image for the book
        /// </summary>
        /// <param name="oclcNumber">The OCLC number representing the material</param>
        /// <returns>Cover Image URI</returns>
        public async static Task<Uri[]> GetOCLCCoverImageUriAsync(string oclcNumber) {
            var baseUri = new Uri(@"https://bucknell.worldcat.org/oclc/");
            var oclcUri = new Uri(baseUri, oclcNumber);
            var request = WebRequest.CreateHttp(oclcUri);
            var response = await request.GetResponseAsync();

            var doc = new HtmlDocument();
            doc.Load(response.GetResponseStream());
            var img = doc.DocumentNode.SelectSingleNode(@"//*[@id='cover']/img");

            var src = img.Attributes["src"].Value;
            return new Uri[] {
                new Uri(baseUri.Scheme + ":" + src),
                new Uri(baseUri.Scheme + ":" + src.Replace("_140.jpg", "_400.jpg")),
                new Uri(baseUri.Scheme + ":" + src.Replace("_140.jpg", "_70.jpg")),
            };
        }

        /// <summary>
        /// Returns the BitMapImage of the cover image
        /// </summary>
        /// <param name="imageUri">URI of the cover image</param>
        /// <returns>Bitmap image of the cover</returns>
        private static async Task<BitmapSource> GetBitmapImage(Uri imageUri) {
            var request = WebRequest.CreateHttp(imageUri);
            var response = await request.GetResponseAsync();

            var ms = new MemoryStream();
            await response.GetResponseStream().CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();

            int stride = (image.Format.BitsPerPixel / 8) * image.PixelWidth;
            var pixels = new byte[stride * image.PixelHeight];
            image.CopyPixels(pixels, stride, 0);

            var average = pixels.Average(b => (decimal?)b);
            if (average < 20 || average > 230) {
                return null;
            } else {
                return image;
            }
        }

        public static BitmapSource DrawPublicationImage(string title, string author) {
            var size = new Size(800, 1200);

            var titleText = new FormattedText(title,
                new System.Globalization.CultureInfo("en-us"),
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                72,
                Brushes.CornflowerBlue);
            titleText.MaxTextWidth = size.Width;
            titleText.TextAlignment = TextAlignment.Center;

            var authorText = new FormattedText(author,
                new System.Globalization.CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                48,
                Brushes.CornflowerBlue);
            authorText.MaxTextWidth = size.Width;
            authorText.TextAlignment = TextAlignment.Center;

            var visual = new DrawingVisual();

            try {
                var ctx = visual.RenderOpen();
                ctx.DrawRectangle(Brushes.White, null, new Rect(size));
                ctx.DrawText(titleText, new Point(0, 100));
                ctx.DrawText(authorText, new Point(0, 100 + titleText.Height + 20));
                ctx.Close();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 92, 92, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }
    }
}
