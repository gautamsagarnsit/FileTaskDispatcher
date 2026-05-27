using log4net;
using MailKit.Net.Smtp;
using MimeKit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace EmailService
{
    public class EmailHandler
    {
        private IConfiguration _emailConfig;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailHandler));
        public EmailHandler(IConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public void Send(string filePath, string subject, string body)
        {
            _logger.Info($"Sending email with subject: {subject} and attachment: {filePath}");
            var message = new MimeMessage();
            var senderName = "Gautam Sagar";
            var sender = Environment.GetEnvironmentVariable("smtp_user");
            var receiver = sender;
            var smtpPass = Environment.GetEnvironmentVariable("smtp_pass");

            _logger.Info($"Email configuration - Sender: {senderName}, {sender}, SMTP Pass: {smtpPass}");

            message.From.Add(new MailboxAddress(senderName, sender));
            message.To.Add(new MailboxAddress("Receiver", receiver));
            message.Subject = subject;

            TextPart text = new TextPart("plain")
            {
                Text = body
            };

            var stream = File.OpenRead(filePath);
            var attachment = new MimePart("application", "pdf")
            {               
                Content = new MimeContent(stream),
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
                _logger.Info("Authenticated to SMTP server successfully.");
                client.Send(message);
                _logger.Info("Email sent successfully.");
                client.Disconnect(true);
            }
            stream.Close();
        }
    }
}
