using log4net;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EmailService
{
    public class EmailHandler
    {
        private IConfigurationRoot _emailConfig;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailHandler));
        public EmailHandler(IConfigurationRoot emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public void Send(string filePath, string subject, string body)
        {
            _logger.Info($"Sending email with subject: {subject} and attachment: {filePath}");
            var message = new MimeMessage();
            var senderName = _emailConfig["SMTP:Name"];
            var sender = _emailConfig["SMTP:user"];
            var receiver = sender;
            var smtpPass = _emailConfig["SMTP:pass"];

            message.From.Add(new MailboxAddress(senderName, sender));
            message.To.Add(new MailboxAddress("Receiver", receiver));
            message.Subject = subject;

            TextPart text = new TextPart("plain")
            {
                Text = body
            };

            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(File.OpenRead(filePath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(filePath)
            };

            var multipart = new Multipart("mixed");
            multipart.Add(text);
            multipart.Add(attachment);

            message.Body = multipart;


            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(sender, smtpPass);

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
