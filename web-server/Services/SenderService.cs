using System.Net.Mail;
using System;
using System.Net;
using System.Runtime.Loader;

namespace web_server.Services
{
    public class SenderService : ISenderService
    {
        public void SendMessage(string address, string message2)
        {

            MailMessage message = new System.Net.Mail.MailMessage();
            string fromEmail = "ivanivanov202311@mail.ru";
            string password = "Y8vH7uFBpbBytddc3HCt";
            string toEmail = address;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail, "Администрация Vitok");
            mail.To.Add(new MailAddress(address)); 
            mail.Subject = "Уведомление от платформы Vitok";
            mail.Body = message2;

            SmtpClient client = new SmtpClient();
            client.Host = "smtp.mail.ru";
            client.Port = 587; 
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(fromEmail, password); 
            try
            {
                client.Send(mail);

            }
            catch (Exception ex)
            {
                return;
            }
        }

    }

}