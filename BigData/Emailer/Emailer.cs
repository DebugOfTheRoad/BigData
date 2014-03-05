using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace BigData.Emailer {
    public class Emailer {
        public static void send_email(string rec, string sub, string msg) {
            MailMessage email = new MailMessage();
            email.To.Add(rec);
            email.Subject = sub;
            email.From = new MailAddress("cec030@bucknell.edu");
            email.Body = msg;
            SmtpClient smtp = new SmtpClient("bucknell.edu.s10a1.psmtp.com", 25);
            //smtp.EnableSsl = true;
            smtp.Timeout = 10000;
            
            // Send the email
            try {
                smtp.Send(email);
            } catch (Exception e) {
                Console.WriteLine("Error: " + e);
            } 
        }
    }
}
