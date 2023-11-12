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

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Username,Password")] MsUser msUser)
        {
            // Handle error when account is registered
            var user = _context.MsUsers.FirstOrDefault(c => c.Username == msUser.Username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Username atau password salah!";
                return RedirectToAction("Login");
            }

            bool verified = _email.verifyPassword(msUser.Password, user.Password);
            if(!verified)
            {
                TempData["ErrorMessage"] = "Username atau password salah!";
                return RedirectToAction("Login");
            }

            // Serialize the user object to a JSON string
            string userJson = JsonConvert.SerializeObject(user);

            // Store the serialized user in the session
            HttpContext.Session.SetString("userAuthentication", userJson);

            // Redirect to verification page
            return RedirectToAction("Index", "User");
        }

        [AuthorizedUser]
        public IActionResult Logout()
        {
            // Retrieve otp from session
            HttpContext.Session.Remove("userAuthentication");

            return RedirectToAction("Login", "User");
        }

        [AuthorizedUser]
        public async Task<IActionResult> Index()
        {
            var usersWithPassword = await _context.MsUsers
                .Where(user => !string.IsNullOrEmpty(user.Password))
                .ToListAsync();

            return View(usersWithPassword);
        }


        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.MsUsers == null)
            {
                return NotFound();
            }

            var msUser = await _context.MsUsers
                .FirstOrDefaultAsync(m => m.IdUser == id);
            if (msUser == null)
            {
                return NotFound();
            }

            return View(msUser);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUser,FullName,Nim,Nidn,Username,Password,Position")] MsUser msUser)
        {
            if (ModelState.IsValid)
            {
                // Generate id user
                string id_user = $"USR{_context.MsUsers.Count()+1}";
                msUser.IdUser = id_user;

                // Hash password
                string hashedPassword = _email.hashPassword(msUser.Password);
                msUser.Password = hashedPassword;

                // Add the user to the database
                _context.Add(msUser);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(msUser);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.MsUsers == null)
            {
                return NotFound();
            }

            var msUser = await _context.MsUsers.FindAsync(id);
            if (msUser == null)
            {
                return NotFound();
            }
            return View(msUser);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdUser,FullName,Nim,Nidn,Username,Password,Position")] MsUser msUser)
        {
            if (id != msUser.IdUser)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(msUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MsUserExists(msUser.IdUser))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(msUser);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.MsUsers == null)
            {
                return NotFound();
            }

            var msUser = await _context.MsUsers
                .FirstOrDefaultAsync(m => m.IdUser == id);
            if (msUser == null)
            {
                return NotFound();
            }

            return View(msUser);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.MsUsers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.MsUsers' is null.");
            }

            var msUser = await _context.MsUsers.FindAsync(id);
            if (msUser != null)
            {
                // Setel kolom password menjadi null
                msUser.Password = null; // Gantilah dengan properti yang sesuai
                _context.Update(msUser); // Tandai entitas sebagai dimodifikasi
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MsUserExists(string id)
        {
          return (_context.MsUsers?.Any(e => e.IdUser == id)).GetValueOrDefault();
        }
    }
}
