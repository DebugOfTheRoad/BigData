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
        public static void emailSend(string username, Publication pub) {
            MailMessage email = new MailMessage();
            email.To.Add(getName(username) + "@bucknell.edu");
            email.Subject = "Here is your eBook: " + pub.Title; // Should probably get something less lame here            
            email.From = new MailAddress("cec030@bucknell.edu");
            email.Body = getMessageBody(getName(username), pub);
            Console.WriteLine(getName(username));
            Console.WriteLine(email.Body);
            SmtpClient smtp = new SmtpClient("bucknell.edu.s10a1.psmtp.com", 25);
            //smtp.EnableSsl = true;
            smtp.Timeout = 10000;
            
            // Send the email
            try {
               // smtp.Send(email);
            } catch (Exception e) {
                Console.WriteLine("Error: " + e);
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
            return "Hey " + name + "! <br> Check out this link! http://bucknell.worldcat.org/oclc/" + pub.OCLCNumber;
        }
        
    }
}
