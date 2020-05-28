using System;
using System.Net.Mail;
using System.ComponentModel;
using System.Net;

namespace Dementor
{
    public class Email
    {
        public Email()
        {
        }

        public string From { get; set; }
        
        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string SmtpAddress { get; set; }

        public int SmtpPort { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }


        public void SendEmail()
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(From);
                mail.To.Add(To);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient(SmtpAddress, SmtpPort))
                {
                    smtp.Credentials = new NetworkCredential(Username, Password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}