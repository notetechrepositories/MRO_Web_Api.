using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MRO_Api.Model;
using System.Net.Mail;
using System.Text;

namespace MRO_Api.Utilities
{
    public class CommunicationUtilities
    {


        /* public async Task<int> SendMail(EmailDtoModel emailDtoModel)
         {
             try
             {
                 // Build email body
                 StringBuilder emailBody = new("<html><body>");
                 emailBody.Append(emailDtoModel.t16_email_html_body);

                 // Create a new MimeMessage
                 var mimeMessage = new MimeMessage();
                 mimeMessage.From.Add(MailboxAddress.Parse(emailDtoModel.from_email));
                 mimeMessage.To.Add(MailboxAddress.Parse(emailDtoModel.to_email));
                 mimeMessage.Subject = emailDtoModel.t16_email_subject;
                 mimeMessage.Body = new TextPart(TextFormat.Html) { Text = emailBody.ToString() };

                 using var smtp = new MailKit.Net.Smtp.SmtpClient();
                 smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                 smtp.Authenticate(emailDtoModel.from_email, emailDtoModel.from_email_password);
                 smtp.Send(mimeMessage);
                 smtp.Disconnect(true);

                 return 1; // Success
             }
             catch (Exception ex)
             {
                 // Log the exception or handle it as needed
                 Console.WriteLine($"Error sending email: {ex.Message}");
                 return 0; // Failure
             }
         }
 */
        public async Task<bool> SendMail(EmailDtoModel emailDtoModel)
        {
            try
            {
                // Build email body
                StringBuilder emailBody = new("<html><body>");
                emailBody.Append(emailDtoModel.t16_email_html_body);
                emailBody.Append(emailDtoModel.signature_content);

                // Create a new MimeMessage
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse(emailDtoModel.from_email));

                // Add CC recipients
                if (!string.IsNullOrEmpty(emailDtoModel.t16_email_cc))
                {
                    foreach (var ccEmail in emailDtoModel.t16_email_cc.Split(','))
                    {
                        mimeMessage.Cc.Add(MailboxAddress.Parse(ccEmail.Trim()));
                    }
                }

                // Add BCC recipients
                if (!string.IsNullOrEmpty(emailDtoModel.t16_email_bcc))
                {
                    foreach (var bccEmail in emailDtoModel.t16_email_bcc.Split(','))
                    {
                        mimeMessage.Bcc.Add(MailboxAddress.Parse(bccEmail.Trim()));
                    }
                }

                mimeMessage.Subject = emailDtoModel.t16_email_subject;
                mimeMessage.Body = new TextPart(TextFormat.Html) { Text = emailBody.ToString() };
               

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(emailDtoModel.from_email, emailDtoModel.from_email_password);
                smtp.Send(mimeMessage);
                smtp.Disconnect(true);

                return true; // Success
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false; // Failure
            }
        }





    }
}
