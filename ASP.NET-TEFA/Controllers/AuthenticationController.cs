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

        // Menampilkan halaman login
        public IActionResult Login()
        {
            return View();
        }

        // Menampilkan halaman registrasi
        public IActionResult Register()
        {
            return View();
        }

        // Proses registrasi pengguna baru
        [HttpPost]
        public async Task<IActionResult> Register([Bind("Email,Name,Phone,Address")] MsCustomer msCustomer)
        {
            bool ModelIsValid = true;

            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Mengecualikan error tertentu berdasarkan nama field
                    if (!(error.ErrorMessage.Contains("IdCustomer")))
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                        ModelIsValid = false;
                    }
                }
            }

            if (ModelIsValid)
            {
                // Handle error jika email kosong
                if (string.IsNullOrWhiteSpace(msCustomer.Email))
                {
                    return View(msCustomer);
                }

                // Handle error jika email sudah terdaftar
                var customer = _context.MsCustomers.FirstOrDefault(c => c.Email == msCustomer.Email);
                if (customer != null)
                {
                    TempData["ErrorMessage"] = "Email already registered";
                    return View(msCustomer);
                }

                // Generate OTP
                string otp = _email.GenerateOtp();

                // Kirim OTP ke email yang disediakan
                _email.SendEmail(msCustomer.Email, otp);

                // Generate id customer
                string id_customer = $"CST{_context.MsCustomers.Count() + 1}";

                // Assign id customer
                msCustomer.IdCustomer = id_customer;

                // Tambahkan customer baru ke database
                _context.Add(msCustomer);
                await _context.SaveChangesAsync();

                // Menampilkan pesan sukses ke view
                TempData["SuccessMessage"] = "Register success";

                // Simpan email & otp ke sesi
                HttpContext.Session.SetString("email", msCustomer.Email);
                HttpContext.Session.SetString("otp", otp);

                // Simpan ke client storage
                TempData["Email"] = msCustomer.Email;

                // Mengarahkan pengguna ke halaman verifikasi
                return RedirectToAction("Verification");
            }
            return View(msCustomer);
        }

        // Proses login pengguna
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email")] MsCustomer msCustomer)
        {
            bool ModelIsValid = true;

            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Mengecualikan error tertentu berdasarkan nama field
                    if (!(error.ErrorMessage.Contains("Customer name") ||
                        error.ErrorMessage.Contains("Address") ||
                        error.ErrorMessage.Contains("Phone") ||
                        error.ErrorMessage.Contains("IdCustomer")))
                    {
                        ModelIsValid = false;
                    }
                }
            }

            if (ModelIsValid)
            {
                // Periksa apakah email sudah terdaftar
                var customer = _context.MsCustomers.FirstOrDefault(c => c.Email == msCustomer.Email);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Email not registered";
                    return RedirectToAction("Login");
                }

                // Generate OTP
                string otp = _email.GenerateOtp();

                // Kirim OTP ke email yang disediakan
                _email.SendEmail(msCustomer.Email, otp);

                // Simpan OTP ke sesi
                HttpContext.Session.SetString("email", msCustomer.Email);
                HttpContext.Session.SetString("otp", otp);

                // Simpan email ke client storage
                TempData["Email"] = msCustomer.Email;

                // Mengarahkan pengguna ke halaman verifikasi
                return RedirectToAction("Verification");
            }
            return View(msCustomer);
        }

        // Menampilkan halaman verifikasi
        public IActionResult Verification()
        {
            // Ambil otp dari sesi
            string otp = HttpContext.Session.GetString("otp");

            // Handle jika tidak ada email dalam sesi
            if (string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // Proses verifikasi OTP
        [HttpPost]
        public async Task<IActionResult> Verification([Bind("Password")] MsCustomer msCustomer)
        {
            string email = HttpContext.Session.GetString("email");
            string otp = HttpContext.Session.GetString("otp");

            // Memeriksa apakah OTP yang dimasukkan oleh pengguna sesuai dengan OTP yang dikirimkan
            if (otp != msCustomer.Password)
            {
                TempData["ErrorMessage"] = "OTP invalid";
                TempData["Email"] = email;
                return RedirectToAction("Verification");
            }

            /// Hapus sesi setelah verifikasi berhasil
            HttpContext.Session.Remove("email");
            HttpContext.Session.Remove("otp");

            // Mengambil data customer dari database berdasarkan email
            var customer = _context.MsCustomers.FirstOrDefault(c => c.Email == email);

            // Mengubah objek customer menjadi string JSON
            string customerJson = JsonConvert.SerializeObject(customer);

            // Menyimpan data customer yang telah di-serialize dalam sesi
            HttpContext.Session.SetString("authentication", customerJson);

            // Mengarahkan pengguna ke halaman utama
            return RedirectToAction("Index", "Home");
        }

        // Proses logout pengguna
        public IActionResult Logout()
        {
            // Menghapus data autentikasi dari sesi
            HttpContext.Session.Remove("authentication");

            // Mengarahkan pengguna ke halaman login
            return RedirectToAction("Login", "Authentication");
        }

        // Menampilkan halaman not found
        public IActionResult NotFound()
        {
            return View();
        }
    }
}