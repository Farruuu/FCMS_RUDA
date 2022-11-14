using System;
using System.Threading.Tasks;
using FCMS_RUDA.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace FCMS_RUDA.Shared
{
    public class Notifications
    {
        public async Task<bool> SendEmail(EmailRequest emailRequest)
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();

                EmailSettings MailConfig = new EmailSettings()
                {
                    Mail = config["MailSettings:Mail"],
                    DisplayName = config["MailSettings:DisplayName"],
                    Password = config["MailSettings:Password"],
                    Host = config["MailSettings:Host"],
                    Port = Convert.ToInt32(config["MailSettings:Port"])
                };

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(MailConfig.DisplayName, MailConfig.Mail));
                emailMessage.To.Add(new MailboxAddress(emailRequest.DisplayName, emailRequest.ToEmail));
                emailMessage.Subject = emailRequest.Subject;
                emailMessage.Body = new TextPart(TextFormat.Html) { Text = emailRequest.Body };

                using var smtp = new SmtpClient();
                smtp.Connect(MailConfig.Host, MailConfig.Port, MailKit.Security.SecureSocketOptions.None);
                await smtp.SendAsync(emailMessage);
                smtp.Disconnect(true);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            #region Commented Code

            //MimeMessage message = new MimeMessage();

            //MailboxAddress from = new MailboxAddress(emailSettings.DisplayName, emailSettings.Mail);
            //message.From.Add(from);

            //MailboxAddress to = new MailboxAddress(emailRequest.DisplayName, emailRequest.ToEmail);
            //message.To.Add(to);

            //message.Subject = emailRequest.Subject;
            //message.Body = emailRequest.Body.ToMessageBody();

            //SmtpClient client = new SmtpClient();

            //client.Connect(emailSettings.Host, emailSettings.Port, true);
            //client.Authenticate(emailSettings.Mail, emailSettings.Password);
            //client.Send(message);
            //client.Disconnect(true);
            //client.Dispose(); 

            #endregion
        }
    }
}
