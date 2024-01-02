using MailKit.Net.Smtp;
using MimeKit;
using System.Data;

namespace Rook01_08.Services.EMail
{
    public class GMailer : IEMailer
    {
        //private readonly IConfiguration _config;

        //public GMailer(IConfiguration config)
        //{
        //    this._config = config;
        //}

        private static String? _mailBoxAddress;
        private static String? _mailBoxPassword;

        public static void SetMailbox(IConfiguration _config)
        {
            _mailBoxAddress = _config["Mailbox:Address"];
            _mailBoxPassword = _config["Mailbox:App_password"];
            if (string.IsNullOrEmpty(_mailBoxAddress) || string.IsNullOrEmpty(_mailBoxPassword))
                throw new NoNullAllowedException("GMail config is null!");
        }
        public static Task SendEMailAsync(string emailTo, string subject, string htmlMessage)
        {
            //var mailBoxAddress = _config["Mailbox:Address"];
            //var mailBoxPassword = _config["Mailbox:App_password"];
            var mailToSend = new MimeMessage();
            mailToSend.From.Add(MailboxAddress.Parse(_mailBoxAddress));
            mailToSend.To.Add(MailboxAddress.Parse(emailTo));
            mailToSend.Subject = subject;
            mailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){ Text = htmlMessage};

            //Sending e-mail
            using (var mailClient = new SmtpClient())
            {
                mailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                mailClient.Authenticate(_mailBoxAddress, _mailBoxPassword);
                mailClient.Send(mailToSend);
                mailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
