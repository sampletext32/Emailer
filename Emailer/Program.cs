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
        private const string SMTP_CREDENTIALS_FILE_NAME = "smtp.txt";
        private const string SENDER_CREDENTIALS_FILE_NAME = "sender_credentials.txt";
        private const string MSG_SENDER_FILE_NAME = "msg_sender.txt";
        private const string MSG_THEME_FILE_NAME = "msg_theme.txt";
        private const string MSG_CONTENT_FILE_NAME = "msg_content.txt";
        private const string RECIPIENTS_FILE_NAME = "recipients.txt";
        private const string ATTACHMENTS_DIRECTORY_NAME = "attachments";

        private static (string host, int port) GetSmtpCredentials()
        {
            string[] smptCredentials = File.ReadAllLines(SMTP_CREDENTIALS_FILE_NAME);
            string smtpHost = smptCredentials[0];
            int smtpPort = Convert.ToInt32(smptCredentials[1]);
            return (smtpHost, smtpPort);
        }

        private static (string username, string password) GetSenderCredentials()
        {
            string[] emailCredentials = File.ReadAllLines(SENDER_CREDENTIALS_FILE_NAME);
            string emailUsername = emailCredentials[0];
            string emailPassword = emailCredentials[1];
            return (emailUsername, emailPassword);
        }

        private static (string email, string nickname) GetMessageSender()
        {
            string[] msgSenderData = File.ReadAllLines(MSG_SENDER_FILE_NAME);
            string msgSenderEmail = msgSenderData[0];
            string msgSenderNickname = msgSenderData[1];
            return (msgSenderEmail, msgSenderNickname);
        }

        private static string GetMessageTheme()
        {
            return File.ReadAllText(MSG_THEME_FILE_NAME);
        }

        private static string GetMessageContent()
        {
            return File.ReadAllText(MSG_CONTENT_FILE_NAME);
        }

        private static List<string> GetRecipients()
        {
            return File.ReadAllLines(RECIPIENTS_FILE_NAME).ToList();
        }

        private static List<Attachment> GetMessageAttachments()
        {
            var files = new DirectoryInfo(ATTACHMENTS_DIRECTORY_NAME).EnumerateFiles();

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

        private static bool CheckFiles()
        {
            if (!File.Exists(SMTP_CREDENTIALS_FILE_NAME))
            {
                Console.WriteLine($"{SMTP_CREDENTIALS_FILE_NAME} not found");
                return false;
            }

            if (!File.Exists(SENDER_CREDENTIALS_FILE_NAME))
            {
                Console.WriteLine($"{SENDER_CREDENTIALS_FILE_NAME} not found");
                return false;
            }

            if (!File.Exists(MSG_SENDER_FILE_NAME))
            {
                Console.WriteLine($"{MSG_SENDER_FILE_NAME} not found");
                return false;
            }

            if (!File.Exists(MSG_THEME_FILE_NAME))
            {
                Console.WriteLine($"{MSG_THEME_FILE_NAME} not found");
                return false;
            }

            if (!File.Exists(MSG_CONTENT_FILE_NAME))
            {
                Console.WriteLine($"{MSG_CONTENT_FILE_NAME} not found");
                return false;
            }

            if (!File.Exists(RECIPIENTS_FILE_NAME))
            {
                Console.WriteLine($"{RECIPIENTS_FILE_NAME} not found");
                return false;
            }

            if (!Directory.Exists(ATTACHMENTS_DIRECTORY_NAME))
            {
                Console.WriteLine($"{ATTACHMENTS_DIRECTORY_NAME} not found");
                return false;
            }

            return true;
        }

        static void Main(string[] args)
        {
            if (!CheckFiles()) return;

            var (smtpHost, smtpPort) = GetSmtpCredentials();
            var (username, password) = GetSenderCredentials();
            var (email, nickname) = GetMessageSender();

            var messageTheme = GetMessageTheme();
            var messageContent = GetMessageContent();

            var recipients = GetRecipients();

            MailAddress from = new MailAddress(email, nickname);

            using (SmtpClient sc = new SmtpClient(smtpHost, smtpPort))
            {
                foreach (var recipient in recipients)
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
            }


            Console.WriteLine("DONE!");
        }
    }
}