using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;

namespace BigData.Management_Interface {
    class ManagementServer {

        public ManagementServer() {
            listener = new HttpListener();
        }

        public Action UpdateDatabaseAction { get; set; }

        /// <summary>
        /// Creates a local server and begins listening for requests.
        /// </summary>
        public void CreateServer() {
            Console.WriteLine("Starting server...");
            listener.Prefixes.Add(ListenPrefix);
            listener.Start();

            responseThread = new Thread(HandleRequests);
            responseThread.Start();
            Console.WriteLine("Server listening on " + ListenPrefix);
        }

        public void StopServer() {
            if (responseThread.IsAlive) {
                responseThread.Abort();
            }

            if (listener.IsListening) {
                Console.WriteLine("Shutting down server...");
                listener.Stop();
                Console.WriteLine("Server shut down");
            }
        }

        HttpListener listener;
        Thread responseThread;

        // Finds the path to the html files in the project space
        // There is probably a better place to put them
        static String ResourcePath = Path.Combine(
            Environment.CurrentDirectory,
            @"Management Interface"
         );

        // Might want to change.
        // If so, also change in confirmation.html
        static String ListenPrefix = "http://+:80/";


        /// <summary>
        /// The method that is called when an http request is made.
        /// On a post request it will update the settings.
        /// On any other type of request it will send the management page.
        /// </summary>
        void HandleRequests() {
            while (true) {
                HttpListenerContext context;

                try {
                    context = listener.GetContext();
                } catch (ThreadAbortException) {
                    // if the thread aborts here, exit gracefully
                    return;
                } catch (Exception ex) {
                    // if we get some other exception, log it, but keep handling requests
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    continue;
                }

                byte[] responsePage;
                Console.WriteLine("Request made... {0}", context.Request.HttpMethod);

                Console.WriteLine(context.Request.Url.AbsolutePath);

                // Check if it is a post or a get request
                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/") {
                    // Read body of post request
                    var reader = new StreamReader(context.Request.InputStream);
                    String formData = reader.ReadToEnd();

                    // Parsed using splits muhahahaha
                    var parsedForm = formData.Split('&');
                    String rss = System.Net.WebUtility.UrlDecode(parsedForm[0].Split('=')[1]);
                    String count = parsedForm[1].Split('=')[1];
                    String wskey = WebUtility.UrlDecode(parsedForm[2].Split('=')[1]);
                    String email = WebUtility.UrlDecode(parsedForm[3].Split('=')[1]);
                    String password = WebUtility.UrlDecode(parsedForm[4].Split('=')[1]);

                    // Make changes to settings if entry is non-blank
                    try {
                        if (rss != "") Properties.Settings.Default.RSSUri = rss;
                        if (count != "") Properties.Settings.Default.Count = Convert.ToInt32(count);
                        if (wskey != "") Properties.Settings.Default.WSKey = wskey;
                        if (email != "") Properties.Settings.Default.MailFrom = email;
                        if (password != "") Properties.Settings.Default.MailPassword = password;

                        // Save changes
                        Properties.Settings.Default.Save();

                        // Set confirmation page
                        responsePage = File.ReadAllBytes(Path.Combine(ResourcePath, "confirmation.html"));
                    } catch (Exception e) {
                        // fugg it
                        Console.WriteLine("Y U No integer?: " + e);

                        // Set fail page
                        responsePage = File.ReadAllBytes(Path.Combine(ResourcePath, "fail.html"));
                    }
                } else if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/") {
                    // Read management html from file
                    string htmlString = File.ReadAllText(Path.Combine(ResourcePath, "management.html"));
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    values["emailsSent"] = "0";
                    values["rssFeed"] = Properties.Settings.Default.RSSUri;
                    values["count"] = Properties.Settings.Default.Count.ToString();
                    values["wsKey"] = Properties.Settings.Default.WSKey;
                    values["email"] = Properties.Settings.Default.MailFrom;
                    string htmlValues = Nustache.Core.Render.StringToString(htmlString, values);

                    // Covert string to byte array
                    responsePage = System.Text.Encoding.UTF8.GetBytes(htmlValues);
                } else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/update") {
                    Console.WriteLine("Refreshing database...");
                    UpdateDatabaseAction();
                    responsePage = File.ReadAllBytes(Path.Combine(ResourcePath, "confirmation.html"));
                } else {
                    responsePage = File.ReadAllBytes(Path.Combine(ResourcePath, "fail.html"));
                }

                // Send the page
                context.Response.ContentType = "text/html";
                context.Response.OutputStream.Write(responsePage, 0, responsePage.Length);
                context.Response.KeepAlive = false;
                context.Response.Close();
                Console.WriteLine("Response handled");
            }
        }



    }
}
