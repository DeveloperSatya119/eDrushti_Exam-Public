using System.Net;
using System.Net.Mail;

namespace eDrushti_Exam.App.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config) => _config = config;

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var smtp = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:Username"] ?? "";
            var pass = _config["Email:Password"] ?? "";
            var from = _config["Email:FromAddress"] ?? user;
            var fromNm = _config["Email:FromName"] ?? "eDrushti Exam";

            using var client = new SmtpClient(smtp, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass)
            };

            var msg = new MailMessage
            {
                From = new MailAddress(from, fromNm),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            msg.To.Add(toEmail);

            await client.SendMailAsync(msg);
        }
    }
}
