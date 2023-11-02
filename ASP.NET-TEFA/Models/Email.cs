using System.Net.Mail;
using System.Net;

namespace ASP.NET_TEFA.Models
{
    public class Email
    {
        public string GenerateOtp()
        {
            Random random = new();
            int otp = random.Next(100000, 999999); // Generate random 6-digit OTP

            return otp.ToString();
        }

        public void SendEmail(string emailId, string otp)
        {
            var fromMail = new MailAddress("rahmatwhy00@gmail.com", "Rahmat"); // set your email    
            var fromEmailpassword = "drqzryswhlsgflsl"; // Set your password     
            var toEmail = new MailAddress(emailId);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Selamat datang di TEACHING FACTORY (TEFA)";
            Message.Body = "<br/> Berikut ini adalah OTP anda: " +otp;

            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }
    }
}