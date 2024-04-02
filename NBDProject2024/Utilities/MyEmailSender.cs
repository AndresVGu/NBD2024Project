using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using NBDProject2024.ViewModels;


namespace NBDProject2024.Utilities
{
    public class MyEmailSender : IMyEmailSender
    {
        private readonly IEmailConfiguration _emailConfigiration;

        public MyEmailSender(IEmailConfiguration emailConfigiration)
        {
            _emailConfigiration = emailConfigiration;
        }

        public async Task SendOneAsync(string name, string email,
            string subject, string htmlMessage)
        {
            if(String.IsNullOrEmpty(name))
            {
                name = email;
            }
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(name, email));
            message.From.Add(new MailboxAddress(
                _emailConfigiration.SmtpFromName, _emailConfigiration.SmtpUsername));

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            //Be careful that the SmtpClient class is one from Mailkit not the Framework.
            using var emailClient = new SmtpClient();
            emailClient.Connect(_emailConfigiration.SmtpServer,
                _emailConfigiration.SmtpPort, false);

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            emailClient.Authenticate(_emailConfigiration.SmtpUsername,
                _emailConfigiration.SmtpPassword);

            await emailClient.SendAsync(message);

            emailClient.Disconnect(true);
        }

        public async Task SendToManyAsync(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAdresses.Select(
                x => new MailboxAddress(x.Name, x.Address)));
            message.From.Add(new MailboxAddress(
               _emailConfigiration.SmtpFromName, _emailConfigiration.SmtpUsername));

            message.Subject = emailMessage.Subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            //Be careful that the SmtpClient class is one from Mailkit not the Framework.
            using var emailClient = new SmtpClient();
            emailClient.Connect(_emailConfigiration.SmtpServer,
                _emailConfigiration.SmtpPort, false);

            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            emailClient.Authenticate(_emailConfigiration.SmtpUsername,
                _emailConfigiration.SmtpPassword);

            await emailClient.SendAsync(message);

            emailClient.Disconnect(true);


        }

    }
}
