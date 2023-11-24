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
    public class InspectionListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InspectionListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Menampilkan daftar pemeriksaan yang terkait dengan suatu pemesanan
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> Index(string idBooking)
        {
            // Mencari pemesanan berdasarkan ID
            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);

            // Jika pemesanan tidak ditemukan, kembalikan NotFound
            if (trsBooking == null)
            {
                return NotFound();
            }

            // Memeriksa status pemesanan dan memberikan pesan jika tidak sesuai kondisi
            if (!(trsBooking.RepairStatus == "INSPECTION LIST" || trsBooking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Inspection list harus dilakukan setelah kontrol atau sebelum eksekusi!";
            }

            // Mengambil daftar pemeriksaan terkait dengan pemesanan
            var trsInspectionLists = _context.TrsInspectionLists
            .Where(t => t.IdBooking ==  idBooking)
            .Include(t => t.IdBookingNavigation)
            .Include(t => t.IdEquipmentNavigation)
            .OrderBy(t => t.IdEquipment)
            .ToListAsync();

            // Menyiapkan data untuk ditampilkan di View
            ViewBag.IdBooking = idBooking;

            return View(await trsInspectionLists);
        }

        // Membuat pemeriksaan baru berdasarkan input formulir
        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection form)
        {
            // Memeriksa pemesanan berdasarkan ID
            string idBooking = form["IdBooking"].ToString();
            var booking = _context.TrsBookings.Find(idBooking);

            // Jika pemesanan tidak ditemukan, kembalikan NotFound
            if (booking == null)
            {
                return NotFound();
            }

            // Memeriksa status pemesanan dan memberikan pesan jika tidak sesuai kondisi
            if (!(booking.RepairStatus == "INSPECTION LIST" || booking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Inspection list harus dilakukan setelah kontrol atau sebelum eksekusi!";
                return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
            }

            // Menghapus data pemeriksaan lama terkait dengan pemesanan
            var existingInspections = _context.TrsInspectionLists.Where(i => i.IdBooking == idBooking);
            _context.TrsInspectionLists.RemoveRange(existingInspections);

            // Menyimpan data pemeriksaan baru berdasarkan input yang diberikan
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("Checklist_"))
                {
                    var idEquipment = key.Replace("Checklist_", "");
                    var checklistValue = form[key].ToString();

                    // Anda juga dapat memeriksa elemen deskripsi dengan nama yang sesuai di sini
                    var descriptionKey = "Description_" + idEquipment;
                    var descriptionValue = form[descriptionKey].ToString();

                    // Konversi nilai checklist ke int (harus sesuai dengan tipe data Checklist di model)
                    int checklist = int.Parse(checklistValue);

                    // Membuat ID pemeriksaan secara acak
                    Random random = new();
                    string idInspection = random.Next(100000, 999999).ToString();

                    // Membuat objek TrsInspectionList baru
                    var data = new TrsInspectionList
                    {
                        IdInspection = idInspection,
                        IdBooking = idBooking,
                        IdEquipment = idEquipment.ToString(),
                        Checklist = checklist,
                        Description = descriptionValue.ToString()
                    };

                    // Menambahkan data pemeriksaan baru ke basis data
                    _context.TrsInspectionLists.Add(data);
                }
            }

            // Mengganti status reparasi dari "EKSEKUSI" menjadi "KONTROL"
            booking.RepairStatus = "EKSEKUSI";

            // Menetapkan waktu mulai reparasi saat ini
            booking.StartRepairTime = DateTime.Now;

            // Menetapkan kemajuan reparasi sebesar 45%
            booking.Progress = 45;

            // Memperbarui data booking di dalam konteks database
            _context.Update(booking);
            _context.SaveChanges();

            // Menyimpan pesan sukses untuk ditampilkan pada tampilan berikutnya
            TempData["SuccessMessage"] = "Inspection list berhasil! Tahapan berlanjut ke inspection list!";

            // Mengarahkan pengguna kembali ke halaman utama Reparasi dengan membawa ID booking
            return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
        }

        // Fungsi berikut digunakan untuk memeriksa apakah data TrsInspectionList dengan ID tertentu sudah ada.
        // Fungsi menerima parameter 'id', yang merupakan ID yang ingin diperiksa keberadaannya.
        private bool TrsInspectionListExists(string id)
        {
            // Menggunakan operator "?." untuk mengakses properti _context.TrsInspectionLists dengan aman,
            // menghindari NullReferenceException jika _context atau TrsInspectionLists null.
            // Metode Any() digunakan untuk memeriksa apakah terdapat elemen dalam koleksi yang memenuhi kondisi tertentu.

            // Ekspresi lambda e => e.IdInspection == id digunakan untuk membandingkan ID TrsInspectionList dengan parameter 'id'.
            // GetValueOrDefault() digunakan untuk mengatasi nilai null yang dapat dihasilkan oleh Any().
            return (_context.TrsInspectionLists?.Any(e => e.IdInspection == id)).GetValueOrDefault();
        }
    }
}
