﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Xml;
using Newtonsoft.Json;
using Nustache.Core;
using System.Windows.Media.Imaging;
using System.IO;



namespace BigData.Emailer {
    public class Emailer {
        public static async void emailSend(string username, Publication pub) {
            var fromAddress = new MailAddress(Properties.Settings.Default.MailFrom, Properties.Settings.Default.MailName);
            MailAddress toAddress;
            try {
                toAddress = new MailAddress(username + "@bucknell.edu", await getFullName(username));
            }
            catch (Exception e) {
                Console.WriteLine("Invalid username. Email not sent.");
                return;
            }
            string fromPassword = Properties.Settings.Default.MailPassword;
            string subject = "Here is your eBook!: " + pub.Title;

            MemoryStream str = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(pub.CoverImage));
            encoder.Save(str);
            str.Position = 0;

            var coverInline = new LinkedResource(str, "image/png");
            string body = getMessageBody(await getFirstName(username), pub, coverInline);

            //Attachment inline = new Attachment(str, "cover");
            //inline.ContentDisposition.Inline = true;
            //inline.ContentType.MediaType = "image/png";
            //inline.ContentType.Name = "cover.png";

            var smtp = new SmtpClient {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress) {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            }) {
                try {
                    //message.Attachments.Add(inline);
                    var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    view.LinkedResources.Add(coverInline);
                    message.AlternateViews.Add(view);
                    await smtp.SendMailAsync(message);
                    Console.WriteLine("Sent to " + toAddress);
                } catch (Exception e) {
                    Console.WriteLine("Error: " + e);
                }
            }
        }

        private static async Task<string> getFullName(String username) {
            var uri = new Uri(@"https://m.bucknell.edu/mobi-web/api/?module=people&q=" + username);
            var request = WebRequest.CreateHttp(uri);
            var response = await request.GetResponseAsync();
            
            var sr = new StreamReader(response.GetResponseStream());
            string json = await sr.ReadToEndAsync();
            List<dynamic> result = JsonConvert.DeserializeObject<List<dynamic>>(json);
            String name = result.First().givenname[0];
            return name;
        }

        private static async Task<string> getFirstName(String username) {
            var uri = new Uri(@"https://m.bucknell.edu/mobi-web/api/?module=people&q=" + username);
            var request = WebRequest.CreateHttp(uri);
            var response = await request.GetResponseAsync();

            var sr = new StreamReader(response.GetResponseStream());
            string json = await sr.ReadToEndAsync();
            List<dynamic> result = JsonConvert.DeserializeObject<List<dynamic>>(json);
            String name = result.First().givenname[0];
            name = name.Split(' ')[0];
            return name;
        }

        private static string getMessageBody(String name, Publication pub, LinkedResource cover) {
            string sTemplate = "<center>Hi {{name}}! <br> Here is the link to {{pubname}}: {{link}}"
                + "<br><a href=\"{{link}}\"><img src=\"cid:{{coverURI}}\" alt=\"\"></a>"
                + "<br><a href=\"{{link2}}\">More information on Bucknell eBooks</a></br>";
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["name"] = name;
            data["pubname"] = pub.Title;
            data["link"] = "http://bucknell.worldcat.org/oclc/" + pub.OCLCNumber;
            data["link2"] = "http://researchbysubject.bucknell.edu/ebooks";
            data["coverURI"] = cover.ContentId;
            return Nustache.Core.Render.StringToString(sTemplate, data);
        }
    }
}
