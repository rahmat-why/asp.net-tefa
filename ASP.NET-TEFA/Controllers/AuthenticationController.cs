using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using ASP.NET_TEFA.Models;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_TEFA.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly Email _email;
        private readonly ApplicationDbContext _context;

        public AuthenticationController(ApplicationDbContext context, Email email)
        {
            _context = context;
            _email = email;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string name, string phone, string address)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                // Handle error when email is empty
                return View();
            }

            // Generate OTP
            string otp = _email.GenerateOtp();

            // Send OTP to the provided email
            _email.SendEmail(email, otp);

            // Generate id customer
            string id_customer = $"CST{_context.MsCustomers.Count()+1}";

            // Create a new customer with the provided details
            var newCustomer = new MsCustomer
            {
                IdCustomer = id_customer,
                Email = email,
                Name = name,
                Phone = phone,
                Address = address,
                Password = otp,
            };

            // Call the CreateCustomer method to add the new customer to the database
            _context.Add(newCustomer);
            await _context.SaveChangesAsync();

            // Redirect to the OTP verification page with email and OTP in query parameters
            return RedirectToAction("Verification", new { email, otp });
        }

        public IActionResult Verification(string email, string otp)
        {
            // Pass email and OTP to the view for verification
            ViewBag.Email = email;
            ViewBag.Otp = otp;

            return View();
        }
    }
}
