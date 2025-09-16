using System;
using System.Net.Mail;
using System.Net;
using System.Windows;
using System.IO;
namespace TechFix
{
    internal class EmailHelper
    {
        public static void SendMessage(string userEmail, string subject, string body)
        {
            string smptServer = "smtp.mail.ru";
            int smptPort = 587;
            string smtpUsername = "email";
            string smtpPassword = "password";

            using (SmtpClient smtpClient = new SmtpClient(smptServer, smptPort))
            {

                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;


                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(userEmail);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    try
                    {
                        smtpClient.Send(mailMessage);
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Ошибка!");
                        return;
                    }
                }
            }
        }
        public static void SendMessageWithAttachment(string userEmail, string subject, string body, string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл для отправки не найден!");
                return;
            }
            string smtpServer = "smtp.mail.ru";
            int smtpPort = 587;
            string smtpUsername = "email";
            string smtpPassword = "password";

            try
            {
                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;

                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(smtpUsername);
                        mailMessage.To.Add(userEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;

                        // Добавляем вложение
                        Attachment attachment = new Attachment(filePath);
                        mailMessage.Attachments.Add(attachment);

                        smtpClient.Send(mailMessage);
                    }
                }
                MessageBox.Show("Письмо с вложением успешно отправлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке письма: {ex.Message}");
            }
        }
    }
}
