using System.Net;
using System.Net.Mail;

namespace Helpers
{
    public static class EmailHelper
    {
        public static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("tainahe176053@fpt.edu.vn", "rrbrxumpgjpdrmtu"),
                EnableSsl = true
            };
            var message = new MailMessage("tainahe176053@fpt.edu.vn", toEmail, subject, body);
            await smtp.SendMailAsync(message);
        }
    }

}
