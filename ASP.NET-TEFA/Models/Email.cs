using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models
{
    public class Email
    {
        private IConfiguration configuration;
        private string username;
        private string password;
        private string name;
        public Email(IConfiguration iConfig) 
        {
            configuration = iConfig;
            username = configuration.GetConnectionString("EmailUsername");
            password = configuration.GetConnectionString("EmailPassword");
            name = configuration.GetConnectionString("EmailName");
        }

        public string GenerateOtp()
        {
            Random random = new();
            int otp = random.Next(100000, 999999); // Generate random 6-digit OTP

            return otp.ToString();
        }

        public void SendEmail(string emailId, [RegularExpression("^[0-9]+$", ErrorMessage = "OTP harus berupa angka")][Required(ErrorMessage = "OTP wajib diisi")] string otp)
        {
            var fromMail = new MailAddress(username, name);
            var fromEmailpassword = password;
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
            Message.Body = "<br/> Berikut ini adalah OTP anda: " + otp;

            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }

        public string hashPassword(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            Console.WriteLine(passwordHash);

            return passwordHash;
        }

        public bool verifyPassword(string password, string hashedPassword)
        {
            bool verified = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            Console.WriteLine(verified);

            return verified;
        }
    }
}