using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Xml;
using Newtonsoft.Json;

namespace BigData.Emailer {
    public class Emailer {
        public static void emailSend(string username, Publication pub){
            var fromAddress = new MailAddress("librarydisplaydonotreply@gmail.com", "Library Digital Display");
            var toAddress = new MailAddress(username + "@bucknell.edu", "To Name");
            string fromPassword = "readingisgood";
            string subject = "Here is your eBook: " + pub.Title; // Should probably get something less lame here            

            string body = getMessageBody(getName(username), pub);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                try
                {
                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }
            }
        }

        private static string getName(String username)
        {
            String URL = @"https://m.bucknell.edu/mobi-web/api/?module=people&q=" + username;
            WebClient client = new WebClient();
            string json = client.DownloadString(URL);
            List<dynamic> result = JsonConvert.DeserializeObject<List<dynamic>>(json);
            String name = result.First().givenname[0];
            name = name.Split(' ')[0];
            return name;
        }

        private static string getMessageBody(String name, Publication pub)
        {
            return "Hey " + name + "! Check out this link! http://bucknell.worldcat.org/oclc/" + pub.OCLCNumber;
        }
        
    }
}
