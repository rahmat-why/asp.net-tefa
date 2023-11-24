using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_TEFA.Models;
using Newtonsoft.Json;

namespace ASP.NET_TEFA.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Menampilkan daftar kendaraan milik pelanggan yang sedang login
        [AuthorizedCustomer]
        public async Task<IActionResult> Index()
        {
            // Mengambil informasi autentikasi pelanggan dari sesi
            string authentication = HttpContext.Session.GetString("authentication");
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            // Memeriksa apakah data pelanggan tidak null
            if (customer != null)
            {
                // Mengambil daftar kendaraan yang dimiliki oleh pelanggan yang sedang login
                var applicationDbContext = _context.MsVehicles
                    .Include(t => t.IdCustomerNavigation)
                    .Where(t => t.IdCustomerNavigation.IdCustomer == customer.IdCustomer).Where(t => t.Classify != "NONAKTIF");

                // Menampilkan halaman Index dengan daftar kendaraan
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                // Jika pelanggan tidak terautentikasi, kembalikan ke halaman utama
                return RedirectToAction(nameof(Index));
            }
        }

        // Menampilkan halaman untuk membuat kendaraan baru
        [AuthorizedCustomer]
        public IActionResult Create()
        {
            return View();
        }

        // Membuat kendaraan baru
        [AuthorizedCustomer]
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Type,Classify,PoliceNumber,Color,Year,VehicleOwner,ChassisNumber,MachineNumber")] MsVehicle msVehicle)
        {
            // Memeriksa validitas model sebelum menyimpan data
            bool ModelIsValid = true;
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Mengecualikan kesalahan tertentu berdasarkan nama field
                    if (!(error.ErrorMessage.Contains("Id Kendaraan") ||
                        error.ErrorMessage.Contains("Id Pelanggan") ||
                        error.ErrorMessage.Contains("IdCustomerNavigation")))
                    {
                        ModelIsValid = false;
                    }
                }
            }

            if (ModelIsValid)
            {
                string authentication = HttpContext.Session.GetString("authentication");
                MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

                if (customer != null)
                {
                    // Membuat ID kendaraan dan menetapkannya ke objek kendaraan
                    string Idvehicle = $"VC{_context.MsVehicles.Count() + 1}";
                    msVehicle.IdVehicle = Idvehicle;
                    // Menetapkan ID pelanggan
                    msVehicle.IdCustomer = customer.IdCustomer;

                    _context.Add(msVehicle);
                    await _context.SaveChangesAsync();
                    // Menampilkan pesan sukses ke view
                    TempData["SuccessMessage"] = "Kendaraan berhasil disimpan!";

                    // Mengarahkan pengguna ke halaman kendaraan
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(msVehicle);
        }

        // Menampilkan halaman untuk mengedit kendaraan
        [AuthorizedCustomer]
        public async Task<IActionResult> Edit(string id)
        {
            // Periksa apakah ID kendaraan tidak kosong atau konteks MsVehicles tidak null
            if (id == null || _context.MsVehicles == null)
            {
                return NotFound();
            }

            // Cari kendaraan berdasarkan ID
            var msVehicle = await _context.MsVehicles.FindAsync(id);

            // Jika kendaraan tidak ditemukan, kembalikan NotFound
            if (msVehicle == null)
                if (msVehicle == null)
                {
                return NotFound();
                }
            // Tampilkan halaman edit kendaraan
            return View(msVehicle);
        }

        // Mengubah data kendaraan yang sudah ada
        [AuthorizedCustomer]
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdVehicle,Type,Classify,PoliceNumber,Color,Year,VehicleOwner,ChassisNumber,MachineNumber,IdCustomer")] MsVehicle msVehicle)
        {
            // Memeriksa validitas model sebelum menyimpan data
            bool ModelIsValid = true;
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    // Mengecualikan kesalahan tertentu berdasarkan nama field
                    if (!(error.ErrorMessage.Contains("Id Kendaraan") ||
                        error.ErrorMessage.Contains("Id Pelanggan") ||
                        error.ErrorMessage.Contains("IdCustomerNavigation")))
                    {
                        ModelIsValid = false;
                    }
                }
            }

            if (ModelIsValid)
            {
                // Memastikan ID kendaraan yang akan diubah sesuai dengan ID yang diterima
                if (id != msVehicle.IdVehicle)
                {
                    return NotFound();
                }

                try
                {
                    Console.WriteLine(msVehicle.IdCustomer);
                    _context.Update(msVehicle);
                    await _context.SaveChangesAsync();
                    // Menampilkan pesan sukses ke view
                    TempData["SuccessMessage"] = "Kendaraan berhasil diperbaharui!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Jika terjadi kesalahan concurrency, cek apakah kendaraan masih ada
                    if (!MsVehicleExists(msVehicle.IdVehicle))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect ke halaman Index setelah berhasil mengubah data
                return RedirectToAction(nameof(Index));
            }
            // Jika model tidak valid, kembalikan halaman edit dengan model kendaraan
            return View(msVehicle);
        }

        // Menampilkan halaman konfirmasi penghapusan kendaraan
        [AuthorizedCustomer]
        public async Task<IActionResult> Delete(string id)
        {
            // Periksa apakah ID kendaraan tidak kosong atau konteks MsVehicles tidak null
            if (id == null || _context.MsVehicles == null)
            {
                // Jika ya, kembalikan NotFound
                return NotFound();
            }

            // Cari kendaraan berdasarkan ID
            var msVehicle = await _context.MsVehicles
                .FirstOrDefaultAsync(m => m.IdVehicle == id);

            // Jika kendaraan tidak ditemukan, kembalikan NotFound
            if (msVehicle == null)
            {
                return NotFound();
            }
            // Tampilkan halaman konfirmasi penghapusan kendaraan
            return View(msVehicle);
        }

        // Menghapus kendaraan dari database
        [AuthorizedCustomer]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Periksa apakah entity set 'ApplicationDbContext.MsVehicles' tidak null
            if (_context.MsVehicles == null)
            {
                return Problem("Entity set 'ApplicationDbContext.MsVehicles'  is null.");
            }

            // Cari kendaraan berdasarkan ID
            var msVehicle = await _context.MsVehicles.FindAsync(id);

            // Jika kendaraan ditemukan, set klasifikasi menjadi "NONAKTIF" dan tandai sebagai dimodifikasi
            if (msVehicle != null)
            {
                msVehicle.Classify = "NONAKTIF";
                _context.MsVehicles.Update(msVehicle);
            }

            // Simpan perubahan ke database
            await _context.SaveChangesAsync();

            // Redirect ke halaman Index setelah penghapusan berhasil
            return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        // Memeriksa keberadaan kendaraan berdasarkan ID di database
        private bool MsVehicleExists(string id)
        {
            // Mengecek apakah konteks MsVehicles tidak null, kemudian memeriksa keberadaan kendaraan berdasarkan ID
            return (_context.MsVehicles?.Any(e => e.IdVehicle == id)).GetValueOrDefault();
        }

        // Menampilkan histori pemesanan dan perbaikan kendaraan
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> History(string id)
        {
            // Memeriksa apakah ID kendaraan atau MsVehicles null
            if (id == null || _context.MsVehicles == null)
            {
                return NotFound();
            }

            // Mengambil data kendaraan termasuk relasi TrsBookings
            var vehicle = await _context.MsVehicles
                .Include(v => v.TrsBookings) 
                .FirstOrDefaultAsync(m => m.IdVehicle == id);

            // Jika kendaraan tidak ditemukan, kembalikan halaman Not Found
            if (vehicle == null)
            {
                return NotFound();
            }

            // Menampilkan halaman histori kendaraan
            return View(vehicle);
        }
    }
}
