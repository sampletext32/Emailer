using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Emailer
{
    class Program
    {
        private static (string host, int port) GetSmtpCredentials()
        {
            string[] smptCredentials = File.ReadAllLines("smtp.txt");
            string smtpHost = smptCredentials[0];
            int smtpPort = Convert.ToInt32(smptCredentials[1]);
            return (smtpHost, smtpPort);
        }

        private static (string username, string password) GetSenderCredentials()
        {
            string[] emailCredentials = File.ReadAllLines("sender_credentials.txt");
            string emailUsername = emailCredentials[0];
            string emailPassword = emailCredentials[1];
            return (emailUsername, emailPassword);
        }

        private static (string email, string nickname) GetMessageSender()
        {
            string[] msgSenderData = File.ReadAllLines("msg_sender.txt");
            string msgSenderEmail = msgSenderData[0];
            string msgSenderNickname = msgSenderData[1];
            return (msgSenderEmail, msgSenderNickname);
        }

        private static string GetMessageTheme()
        {
            return File.ReadAllText("msg_theme.txt");
        }

        private static string GetMessageContent()
        {
            return File.ReadAllText("msg_content.txt");
        }

        private static List<string> GetRecipients()
        {
            return File.ReadAllLines("recipients.txt").ToList();
        }

        private static List<Attachment> GetMessageAttachments()
        {
            string folder = "attachments";
            var files = new DirectoryInfo(folder).EnumerateFiles();

            List<Attachment> attachments = new List<Attachment>();

            foreach (var file in files)
            {
                var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                attachments.Add(new Attachment(fileStream, file.Name, MediaTypeNames.Application.Octet));
            }

            return attachments;
        }

        private static void AppendAttachments(MailMessage message, List<Attachment> attachments)
        {
            foreach (var attachment in attachments)
            {
                message.Attachments.Add(attachment);
            }
        }

        private static void FreeAttachmentsStreams(List<Attachment> attachments)
        {
            foreach (var attachment in attachments)
            {
                attachment.ContentStream.Close();
            }
        }

        static void Main(string[] args)
        {
            var (smtpHost, smtpPort) = GetSmtpCredentials();
            var (username, password) = GetSenderCredentials();
            var (email, nickname) = GetMessageSender();

            var messageTheme = GetMessageTheme();
            var messageContent = GetMessageContent();

            var recipients = GetRecipients();

            MailAddress from = new MailAddress(email, nickname);

            recipients.ForEach(recipient =>
            {
                using (SmtpClient sc = new SmtpClient(smtpHost, smtpPort))
                {
                    sc.EnableSsl = true;
                    sc.Credentials = new NetworkCredential(username, password);

                    MailAddress to = new MailAddress(recipient);
                    using (MailMessage mm = new MailMessage(from, to))
                    {
                        mm.Subject = messageTheme;
                        mm.Body = messageContent;
                        mm.IsBodyHtml = false;
                        var messageAttachments = GetMessageAttachments();
                        AppendAttachments(mm, messageAttachments);
                        sc.Send(mm);
                        FreeAttachmentsStreams(messageAttachments);
                    }

                    Console.WriteLine($"SENT TO {recipient}");
                }
            });


            Console.WriteLine("DONE!");
        }
    }
}