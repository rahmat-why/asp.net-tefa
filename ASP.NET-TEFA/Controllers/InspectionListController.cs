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

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> Index(string idBooking)
        {
            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);
            if (trsBooking == null)
            {
                return NotFound();
            }

            if (!(trsBooking.RepairStatus == "INSPECTION LIST" || trsBooking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Inspection list harus dilakukan setelah kontrol atau sebelum eksekusi!";
            }

            var trsInspectionLists = _context.TrsInspectionLists
            .Where(t => t.IdBooking ==  idBooking)
            .Include(t => t.IdBookingNavigation)
            .Include(t => t.IdEquipmentNavigation)
            .OrderBy(t => t.IdEquipment)
            .ToListAsync();

            ViewBag.IdBooking = idBooking;

            return View(await trsInspectionLists);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection form)
        {
            // check booking
            string idBooking = form["IdBooking"].ToString();
            var booking = _context.TrsBookings.Find(idBooking);
            if (booking == null)
            {
                return NotFound();
            }

            if (!(booking.RepairStatus == "INSPECTION LIST" || booking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Inspection list harus dilakukan setelah kontrol atau sebelum eksekusi!";
                return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
            }

            // Lakukan penghapusan data lama
            var existingInspections = _context.TrsInspectionLists.Where(i => i.IdBooking == idBooking);
            _context.TrsInspectionLists.RemoveRange(existingInspections);

            // Simpan data baru berdasarkan input yang diberikan
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

                    Random random = new();
                    string idInspection = random.Next(100000, 999999).ToString();
                    var data = new TrsInspectionList
                    {
                        IdInspection = idInspection,
                        IdBooking = idBooking,
                        IdEquipment = idEquipment.ToString(),
                        Checklist = checklist,
                        Description = descriptionValue.ToString()
                    };

                    // Simpan data ke basis data
                    _context.TrsInspectionLists.Add(data);
                }
            }

            // Ubah status reparasi menjadi KONTROL
            booking.RepairStatus = "EKSEKUSI";
            booking.StartRepairTime = DateTime.Now;
            booking.Progress = 45;

            _context.Update(booking);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Inspection list berhasil! Tahapan berlanjut ke inspection list!";

            return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
        }

        private bool TrsInspectionListExists(string id)
        {
          return (_context.TrsInspectionLists?.Any(e => e.IdInspection == id)).GetValueOrDefault();
        }
    }
}
