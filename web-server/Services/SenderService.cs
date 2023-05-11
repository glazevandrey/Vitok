using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class SenderService : ISenderService
    {
        private static IServiceProvider _serviceProvider;
        public SenderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task SendMessage(int id, string message2)
        {
            var scope = _serviceProvider.CreateScope();

            DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var user = await db.Users.FirstOrDefaultAsync(m=>m.UserId == id);
            db.Entry(user).State = EntityState.Detached;
            db.Dispose();
            if (user == null)
            {
                return;
            }
            var address = user.Email;

            MailMessage message = new System.Net.Mail.MailMessage();
            string fromEmail = "ivanivanov202311@mail.ru";
            string password = "yiKbKgPPjQmEMHRfeMuR";
            // string password = "Xs3-6jr-fAH-TdM";

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