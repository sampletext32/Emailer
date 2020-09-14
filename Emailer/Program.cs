using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;

namespace Emailer
{
    class Program
    {
        private const string SmtpCredentialsFileName = "smtp.txt";
        private const string SenderCredentialsFileName = "sender_credentials.txt";
        private const string MsgSenderFileName = "msg_sender.txt";
        private const string MsgThemeFileName = "msg_theme.txt";
        private const string MsgContentFileName = "msg_content.txt";
        private const string RecipientsFileName = "recipients.txt";
        private const string FailedRecipientsFileName = "failed.txt";
        private const string AttachmentsDirectoryName = "attachments";

        private static (string host, int port) GetSmtpCredentials()
        {
            var smtpCredentials = File.ReadAllLines(SmtpCredentialsFileName);
            var smtpHost = smtpCredentials[0];
            var smtpPort = Convert.ToInt32(smtpCredentials[1]);
            return (smtpHost, smtpPort);
        }

        private static (string username, string password) GetSenderCredentials()
        {
            var emailCredentials = File.ReadAllLines(SenderCredentialsFileName);
            var emailUsername = emailCredentials[0];
            var emailPassword = emailCredentials[1];
            return (emailUsername, emailPassword);
        }

        private static (string email, string nickname) GetMessageSender()
        {
            var msgSenderData = File.ReadAllLines(MsgSenderFileName);
            var msgSenderEmail = msgSenderData[0];
            var msgSenderNickname = msgSenderData[1];
            return (msgSenderEmail, msgSenderNickname);
        }

        private static string GetMessageTheme()
        {
            return File.ReadAllText(MsgThemeFileName);
        }

        private static string GetMessageContent()
        {
            return File.ReadAllText(MsgContentFileName);
        }

        private static List<string> GetRecipients()
        {
            return File.ReadAllLines(RecipientsFileName).ToList();
        }

        private static List<Attachment> GetMessageAttachments()
        {
            var files = new DirectoryInfo(AttachmentsDirectoryName).EnumerateFiles();

            var attachments = new List<Attachment>();
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
            if (!File.Exists(SmtpCredentialsFileName))
            {
                Console.WriteLine($"{SmtpCredentialsFileName} not found");
                return false;
            }

            if (!File.Exists(SenderCredentialsFileName))
            {
                Console.WriteLine($"{SenderCredentialsFileName} not found");
                return false;
            }

            if (!File.Exists(MsgSenderFileName))
            {
                Console.WriteLine($"{MsgSenderFileName} not found");
                return false;
            }

            if (!File.Exists(MsgThemeFileName))
            {
                Console.WriteLine($"{MsgThemeFileName} not found");
                return false;
            }

            if (!File.Exists(MsgContentFileName))
            {
                Console.WriteLine($"{MsgContentFileName} not found");
                return false;
            }

            if (!File.Exists(RecipientsFileName))
            {
                Console.WriteLine($"{RecipientsFileName} not found");
                return false;
            }

            if (!Directory.Exists(AttachmentsDirectoryName))
            {
                Console.WriteLine($"{AttachmentsDirectoryName} not found");
                return false;
            }

            return true;
        }

        private static void Main()
        {
            if (!CheckFiles())
            {
                Console.WriteLine("Some of the required files missing! Abort.");
                Console.ReadKey();
                return;
            }

            var (smtpHost, smtpPort) = GetSmtpCredentials();
            var (username, password) = GetSenderCredentials();
            var (email, nickname) = GetMessageSender();

            var messageTheme = GetMessageTheme();
            var messageContent = GetMessageContent();

            var recipients = GetRecipients();

            var failedRecipients = new List<string>();

            var mailAddressFrom = new MailAddress(email, nickname);

            using var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(username, password);
            foreach (var recipient in recipients)
            {
                var mailAddressTo = new MailAddress(recipient);
                using var mailMessage = new MailMessage(mailAddressFrom, mailAddressTo);
                mailMessage.Subject = messageTheme;
                mailMessage.Body = messageContent;
                mailMessage.IsBodyHtml = false;
                var messageAttachments = GetMessageAttachments();
                AppendAttachments(mailMessage, messageAttachments);
                try
                {
                    smtpClient.Send(mailMessage);

                    Console.WriteLine($"SUCCESS TO {recipient}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR TO {recipient} - {ex.Message}");
                    failedRecipients.Add(recipient);
                }

                FreeAttachmentsStreams(messageAttachments);

                Thread.Sleep(300);
            }

            if (failedRecipients.Count != 0)
            {
                File.WriteAllLines(FailedRecipientsFileName, failedRecipients);
                Console.WriteLine($"{FailedRecipientsFileName} formed with {failedRecipients.Count} elements");
            }

            Console.WriteLine("DONE!");
            Console.ReadKey();
        }
    }
}