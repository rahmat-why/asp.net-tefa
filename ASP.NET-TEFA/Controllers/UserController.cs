using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_TEFA.Models;
using Newtonsoft.Json;
using System.Security.Policy;

namespace ASP.NET_TEFA.Controllers
{
    public class UserController : Controller
    {
        private readonly Email _email;
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context, Email email)
        {
            _context = context;
            _email = email;
        }

        // Menampilkan halaman login
        public async Task<IActionResult> Login()
        {
            return View();
        }

        // Proses login pengguna
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Username,Password")] MsUser msUser)
        {
            // Handle kesalahan ketika akun belum terdaftar
            var user = _context.MsUsers.FirstOrDefault(c => c.Username == msUser.Username);
            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                TempData["ErrorMessage"] = "Username atau password salah!";
                return RedirectToAction("Login");
            }

            // Verifikasi kata sandi menggunakan metode verifyPassword dari objek _email
            bool verified = _email.verifyPassword(msUser.Password, user.Password);
            if(!verified)
            {
                TempData["ErrorMessage"] = "Username atau password salah!";
                return RedirectToAction("Login");
            }

            // Serialize objek pengguna menjadi string JSON
            string userJson = JsonConvert.SerializeObject(user);

            // Simpan pengguna yang telah diserialkan dalam sesi
            HttpContext.Session.SetString("userAuthentication", userJson);

            // Redirect ke halaman verifikasi
            return RedirectToAction("History", "Booking");
        }

        // Logout pengguna
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public IActionResult Logout()
        {
            // Hapus autentikasi pengguna dari sesi
            HttpContext.Session.Remove("userAuthentication");

            // Redirect ke halaman login
            return RedirectToAction("Login", "User");
        }

        // Menampilkan daftar pengguna dengan password
        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> Index()
        {
            // Mengambil daftar pengguna dari database yang memiliki password
            var usersWithPassword = await _context.MsUsers
                .Where(user => !string.IsNullOrEmpty(user.Password) && user.Position == "HEAD MECHANIC")
                .ToListAsync();

            return View(usersWithPassword);
        }

        // Menampilkan halaman untuk membuat pengguna baru
        [AuthorizedUser("SERVICE ADVISOR")]
        public IActionResult Create()
        {
            return View();
        }

        // Membuat pengguna baru
        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Nim,Username,Password,Position")] MsUser msUser)
        {
            // Validasi ModelState
            bool ModelIsValid = true;

            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Mengecualikan validasi Password wajib diisi
                    if (!(error.ErrorMessage.Contains("Password wajib diisi") || 
                        error.ErrorMessage.Contains("The IdUser field is required.")))
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                        ModelIsValid = false;
                    }
                }
            }

            // Jika model valid
            if (ModelIsValid)
            {
                // Validasi keunikan Username
                var user = await _context.MsUsers.Where(t => t.Username == msUser.Username).ToListAsync();
                if (user.Count > 0)
                {
                    TempData["ErrorMessage"] = "Username sudah digunakan!";
                    return View(msUser);
                }

                // Validasi keunikan NIM
                var pw = await _context.MsUsers.Where(t => t.Nim == msUser.Nim).ToListAsync();
                if (pw.Count > 0)
                {
                    TempData["ErrorMessage"] = "NIM sudah digunakan!";
                    return View(msUser);
                }

                // Generate ID pengguna
                string id_user = $"USR{_context.MsUsers.Count() + 1}";
                msUser.IdUser = id_user;

                // Hash password
                string hashedPassword = _email.hashPassword(msUser.Password);
                msUser.Password = hashedPassword;

                // Tambahkan pengguna ke database
                _context.Add(msUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data berhasil disimpan!";
                return RedirectToAction("Index", "User");
            }

            // Jika model tidak valid, kembali ke halaman pengeditan dengan model pengguna
            return View(msUser);
        }

        // Menampilkan halaman untuk mengedit pengguna
        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> Edit(string id)
        {
            // Memeriksa apakah id pengguna atau tabel pengguna tidak ditemukan
            if (id == null || _context.MsUsers == null)
            {
                return NotFound();
            }

            // Mengambil data pengguna berdasarkan id
            var msUser = await _context.MsUsers.FindAsync(id);

            // Jika pengguna tidak ditemukan, kembalikan respons 'Not Found'
            if (msUser == null)
            {
                return NotFound();
            }

            // Menampilkan halaman untuk mengedit pengguna
            return View(msUser);
        }

        // Mengubah data pengguna yang sudah ada
        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdUser,FullName,Nim,Nidn,Username,Password,Position")] MsUser msUser)
        {
            // Validasi ModelState
            bool ModelIsValid = true;

            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Mengecualikan validasi Password wajib diisi
                    if (!(error.ErrorMessage.Contains("Password wajib diisi")))
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                        ModelIsValid = false;
                    }
                }
            }

            // Jika model valid
            if (ModelIsValid)
            {
                // Mengambil pengguna berdasarkan ID dari database
                var user = await _context.MsUsers.FindAsync(id);

                // Jika pengguna tidak ditemukan, kembalikan respons 'Not Found'
                if (user == null)
                {
                    return NotFound();
                }

                // Memperbarui seluruh atribut pengguna
                user.FullName = msUser.FullName;
                user.Nim = msUser.Nim;

                // Jika terdapat kata sandi baru
                if (msUser.Password != null)
                {
                    user.Password = _email.hashPassword(msUser.Password);
                }

                // Memperbarui pengguna dalam database
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Menampilkan pesan sukses ke view
                TempData["SuccessMessage"] = "Data berhasil diubah!";

                // Redirect ke halaman Index
                return RedirectToAction(nameof(Index));
            }

            // Jika model tidak valid, kembali ke halaman pengeditan dengan model pengguna
            return View(msUser);
        }

        // Menampilkan halaman konfirmasi penghapusan pengguna
        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.MsUsers == null)
            {
                // Memeriksa apakah ID atau set data pengguna tidak tersedia
                return NotFound();
            }

            // Mengambil pengguna berdasarkan ID dari database
            var msUser = await _context.MsUsers
                .FirstOrDefaultAsync(m => m.IdUser == id);

            // Jika pengguna tidak ditemukan, kembalikan respons 'Not Found'
            if (msUser == null)
            {
                return NotFound();
            }

            // Menampilkan halaman konfirmasi penghapusan pengguna
            return View(msUser);
        }

        // Menghapus pengguna dari database
        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Memeriksa apakah set data pengguna tidak tersedia
            if (_context.MsUsers == null)
            {
                // Jika tidak tersedia, kembalikan respons 'Problem'
                return Problem("Entity set 'ApplicationDbContext.MsUsers' is null.");
            }

            // Mengambil pengguna berdasarkan ID dari database
            var msUser = await _context.MsUsers.FindAsync(id);

            // Jika pengguna ditemukan
            if (msUser != null)
            {
                // Setel kolom password menjadi null
                msUser.Password = "";

                // Tandai entitas sebagai dimodifikasi
                _context.Update(msUser);

                // Simpan perubahan ke database
                await _context.SaveChangesAsync();
            }

            // Redirect ke halaman daftar pengguna setelah penghapusan
            return RedirectToAction(nameof(Index));
        }

        // Memeriksa keberadaan pengguna berdasarkan ID
        private bool MsUserExists(string id)
        {
            // Menggunakan ekspresi null-conditional (?) untuk memeriksa keberadaan set data pengguna
            return (_context.MsUsers?.Any(e => e.IdUser == id)).GetValueOrDefault();
        }
    }
}
