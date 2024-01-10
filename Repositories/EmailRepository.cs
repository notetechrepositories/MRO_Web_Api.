using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MRO_Api.IRepository;
using MRO_Api.Model;
using System.Net.Mail;
using System.Text;

namespace MRO_Api.Repositories
{
    public class EmailRepository:IEmailRepository
    {



        /*   public async Task<string> GenerateOtp()
           {
               Random random = new Random();
               string pin = new string(Enumerable.Repeat("0123456789", 4)
                   .Select(s => s[random.Next(s.Length)]).ToArray());
               return pin;
           }


           public async Task<string> GenerateOtpTime()
           {
               string currentTime = DateTime.Now.ToString();
               return currentTime;
           }



           public async Task SendEmail(EmailDtoModel emailDtoModel)
           {

               try
               {
                   // email body
                   StringBuilder emailBody = new("<html><body>");
                   emailBody.Append(" <p> Hi " + emailDtoModel.first_name + emailDtoModel.last_name + ",</p>");
                   emailBody.Append("<p>");
                   emailBody.Append("You PIN is " + emailDtoModel.pin + "<br>Please change it after the sign-in.</p>");
                   emailBody.Append("<p>Thanks & Regards,<br>");


                   // Combine the uploads folder path with the generated file name
                   string webRootPath = Directory.GetCurrentDirectory() + "\\Media\\Icons\\logo.png";
                   emailBody.Append("<img  src='C: \\Users\\ADMIN\\OneDrive\\MRO\\Media\\Icons\\logo.png'> <br>");
                   emailBody.Append("KSEB transformer management system<br>");
                   emailBody.Append("KSEB Kerala</p>");
                   emailBody.Append("Powered by Notetech Software</p>");


                   emailBody.Append("</body></html>");
                   // creates a new MimeMessage
                   var email = new MimeMessage();
                   email.From.Add(MailboxAddress.Parse(_IConfiguration.GetSection("SMTPSettings:EmailUserName").Value));
                   email.To.Add(MailboxAddress.Parse(Email.email));
                   email.From.Add(MailboxAddress.Parse(_IConfiguration.GetSection("SMTPSettings:EmailUserName").Value));
                   email.To.Add(MailboxAddress.Parse(emailDtoModel.email));
                   email.Subject = "password reset notification";
                   email.Body = new TextPart(TextFormat.Html) { Text = emailBody.ToString() };

                   // send email
                   using var smtp = new SmtpClient();
                   smtp.Connect(_IConfiguration.GetSection("SMTPSettings:EmailHost").Value, 587, SecureSocketOptions.StartTls);
                   smtp.Authenticate(_IConfiguration.GetSection("SMTPSettings:EmailUserName").Value, _IConfiguration.GetSection("SMTPSettings:EmailPassword").Value);

                   smtp.Send(email);
                   smtp.Disconnect(true);
               }
               catch (Exception e)
               {
                   throw new Exception(e.Message);
               }


           }*/




        public async Task<int> SendMail(EmailDtoModel emailDtoModel)
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









    }
}
