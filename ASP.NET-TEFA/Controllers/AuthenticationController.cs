using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using ASP.NET_TEFA.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public async Task<IActionResult> Register([Bind("Email,Name,Phone,Address")] MsCustomer msCustomer)
        {
            bool ModelIsValid = true;

            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Exclude specific errors based on field names
                    if (!(error.ErrorMessage.Contains("Nama Customer") ||
                        error.ErrorMessage.Contains("Alamat Lengkap") ||
                        error.ErrorMessage.Contains("No Telepon") ||
                        error.ErrorMessage.Contains("IdCustomer")))
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                        ModelIsValid = false;
                    }
                }
            }

            if (ModelIsValid)
            {
                // Handle error when email is empty
                if (string.IsNullOrWhiteSpace(msCustomer.Email))
                {
                    return View();
                }

                // Handle error when email is registered
                var customer = _context.MsCustomers.FirstOrDefault(c => c.Email == msCustomer.Email);
                if (customer != null)
                {
                    TempData["ErrorMessage"] = "Email sudah terdaftar!";
                    return RedirectToAction("Register");
                }

                // Generate OTP
                string otp = _email.GenerateOtp();

                // Send OTP to the provided email
                _email.SendEmail(msCustomer.Email, otp);

                // Generate id customer
                string id_customer = $"CST{_context.MsCustomers.Count() + 1}";

                // Assign id customer
                msCustomer.IdCustomer = id_customer;

                // Create new customer to the database
                _context.Add(msCustomer);
                await _context.SaveChangesAsync();

                // Send alert success message to view
                TempData["SuccessMessage"] = "Registrasi berhasil!";

                // Save email & otp to session
                HttpContext.Session.SetString("email", msCustomer.Email);
                HttpContext.Session.SetString("otp", otp);

                // Save to client storage
                TempData["Email"] = msCustomer.Email;

                // Redirect to verification page
                return RedirectToAction("Verification");
            }
            return View(msCustomer);
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email")] MsCustomer msCustomer)
        {
            bool ModelIsValid = true;

            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Exclude specific errors based on field names
                    if (!(error.ErrorMessage.Contains("Nama Customer") ||
                        error.ErrorMessage.Contains("Alamat Lengkap") ||
                        error.ErrorMessage.Contains("No Telepon") ||
                        error.ErrorMessage.Contains("IdCustomer")))
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                        ModelIsValid = false;
                    }
                }
            }

            if (ModelIsValid)
            {
                // Check if email is registered
                var customer = _context.MsCustomers.FirstOrDefault(c => c.Email == msCustomer.Email);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Email belum terdaftar!";
                    return RedirectToAction("Login");
                }

                // Generate OTP
                string otp = _email.GenerateOtp();

                // Send OTP to the provided email
                _email.SendEmail(msCustomer.Email, otp);

                // Save OTP to session
                HttpContext.Session.SetString("email", msCustomer.Email);
                HttpContext.Session.SetString("otp", otp);

                // Save email to client storage
                TempData["Email"] = msCustomer.Email;

                // Redirect to verification page
                return RedirectToAction("Verification");
            }
            return View(msCustomer);
        }


        public IActionResult Verification()
        {
            // Retrieve otp from session
            string otp = HttpContext.Session.GetString("otp");

            // Handle if no email in session
            if (string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verification([Bind("Password")] MsCustomer msCustomer)
        {
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            if (true) //ModelState.IsValid
            {
                // Retrieve otp from session
                string email = HttpContext.Session.GetString("email");
                string otp = HttpContext.Session.GetString("otp");

                if (otp != msCustomer.Password)
                {
                    TempData["ErrorMessage"] = "OTP tidak valid!";
                    TempData["Email"] = email;
                    return RedirectToAction("Verification");
                }

                // Remove the session after successful verification
                HttpContext.Session.Remove("email");
                HttpContext.Session.Remove("otp");

                // Retrieve customer
                var customer = _context.MsCustomers.FirstOrDefault(c => c.Email == email);

                // Serialize the customer object to a JSON string
                string customerJson = JsonConvert.SerializeObject(customer);

                // Store the serialized customer in the session
                HttpContext.Session.SetString("authentication", customerJson);

                // Redirect to home page
                return RedirectToAction("Index", "Home");
            }
            return View(msCustomer);
        }

        public IActionResult Logout()
        {
            // Retrieve otp from session
            HttpContext.Session.Remove("authentication");

            return RedirectToAction("Login", "Authentication");
        }

        public IActionResult NotFound()
        {
            return View();
        }
    }
}