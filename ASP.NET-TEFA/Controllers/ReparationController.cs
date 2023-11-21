using ASP.NET_TEFA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;

namespace ASP.NET_TEFA.Controllers
{
    public class ReparationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReparationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> Index(string idBooking)
        {
            var booking = await _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .FirstOrDefaultAsync(c => c.IdBooking == idBooking);

            return View(booking);
        }

        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> FormMethod(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if(booking.Progress > 0)
            {
                TempData["ErrorMessage"] = "Perubahan metode tidak dapat dilakukan ketika progres sudah berjalan!";
            }

            return View(booking);
        }

        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost]
        public async Task<IActionResult> FormMethod([Bind("IdBooking, RepairMethod, FinishEstimationTime")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }

            string repair_status;
            if (booking.RepairMethod == "TEFA")
            {
                repair_status = "PERENCANAAN";
            }
            else
            {
                repair_status = "MULAI";
            }

            booking.RepairMethod = trsBooking.RepairMethod;
            booking.FinishEstimationTime = trsBooking.FinishEstimationTime;
            booking.RepairStatus = repair_status;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormPlan(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (!(booking.RepairStatus == "PERENCANAAN" || booking.RepairStatus == "KEPUTUSAN"))
            {
                TempData["ErrorMessage"] = "Perencanaan harus dilakukan sebelum keputusan!";
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormPlan([Bind("IdBooking, RepairDescription, ReplacementPart")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }

            if (!(booking.RepairStatus == "PERENCANAAN" || booking.RepairStatus == "KEPUTUSAN"))
            {
                TempData["ErrorMessage"] = "Perencanaan harus dilakukan setelah info proyek atau sebelum keputusan!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.RepairDescription = trsBooking.RepairDescription;
            booking.ReplacementPart = trsBooking.ReplacementPart;
            booking.RepairStatus = "KEPUTUSAN";
            booking.Progress = 10;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Perencanaan berhasil! Tahapan berlanjut ke keputusan!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser]
        public async Task<IActionResult> FormDecision(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (!(booking.RepairStatus == "KEPUTUSAN" || booking.RepairStatus == "INSPECTION LIST"))
            {
                TempData["ErrorMessage"] = "Keputusan harus dilakukan setelah perencanaan atau sebelum eksekusi!";
            }

            return View(booking);
        }

        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormDecision([Bind("IdBooking, Price")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }

            if (!(booking.RepairStatus == "KEPUTUSAN" || booking.RepairStatus == "INSPECTION LIST"))
            {
                TempData["ErrorMessage"] = "Keputusan harus dilakukan setelah perencanaan atau sebelum eksekusi!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            // Lakukan penghapusan data lama
            var existingInspections = _context.TrsInspectionLists.Where(i => i.IdBooking == trsBooking.IdBooking);
            _context.TrsInspectionLists.RemoveRange(existingInspections);

            var equipment = await _context.MsEquipments.Where(t => t.IsActive == 1).ToListAsync();
            for (int i = 0; i < equipment.Count; i++)
            {
                Random random = new();
                string idInspection = random.Next(100000, 999999).ToString();
                var data = new TrsInspectionList
                {
                    IdInspection = idInspection,
                    IdBooking = trsBooking.IdBooking,
                    IdEquipment = equipment[i].IdEquipment,
                    Checklist = 1,
                    Description = null
                };

                _context.TrsInspectionLists.Add(data);
            }

            booking.Price = trsBooking.Price;
            booking.RepairStatus = "INSPECTION LIST";
            booking.Progress = 20;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Keputusan berhasil! Tahapan berlanjut ke inspection list!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormStartExecution(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (booking.StartRepairTime != null)
            {
                TempData["ErrorMessage"] = "Eksekusi sudah dimulai pada: "+booking.StartRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormStartExecution([Bind("IdBooking, EndRepairTime")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (booking.StartRepairTime != null)
            {
                TempData["ErrorMessage"] = "Eksekusi sudah dimulai pada: "+booking.StartRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
                return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
            }

            booking.StartRepairTime = DateTime.Now;
            booking.RepairStatus = "EKSEKUSI";
            booking.Progress = 50;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mulai berhasil! Tahapan berlanjut ke eksekusi!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormFinishExecution(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (booking.EndRepairTime != null)
            {
                TempData["ErrorMessage"] = "Eksekusi sudah diselesaikan pada: "+booking.EndRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormFinishExecution([Bind("IdBooking, EndRepairTime")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (booking.EndRepairTime != null)
            {
                TempData["ErrorMessage"] = "Eksekusi sudah diselesaikan pada: "+booking.EndRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            string repair_status;
            int progress;
            if (booking.RepairMethod == "TEFA")
            {
                repair_status = "KONTROL";
                progress = 70;
            }
            else
            {
                repair_status = "SELESAI";
                progress = 100;
            }

            booking.EndRepairTime = DateTime.Now;
            booking.RepairStatus = repair_status;
            booking.Progress = progress;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Keputusan berhasil! Tahapan berlanjut ke inspection list!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormControl(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (!(booking.RepairStatus == "KONTROL" || booking.RepairStatus == "EVALUASI"))
            {
                TempData["ErrorMessage"] = "Kontrol harus dilakukan setelah keputusan atau sebelum evaluasi!";
            }

            return View(booking);
        }

        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormControl(IFormCollection form)
        {
            string idBooking = form["IdBooking"].ToString();
            int? control = int.TryParse(form["control"], out var parsedValue) ? parsedValue : (int?)null;

            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (!(booking.RepairStatus == "KONTROL" || booking.RepairStatus == "EVALUASI"))
            {
                TempData["ErrorMessage"] = "Kontrol harus dilakukan setelah keputusan atau sebelum evaluasi!";
                return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
            }

            booking.Control = control;
            booking.RepairStatus = "EVALUASI";
            booking.Progress = 90;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kontrol berhasil! Tahapan berlanjut ke evaluasi!";

            return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormEvaluation(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            if (booking.RepairStatus != "EVALUASI")
            {
                TempData["ErrorMessage"] = "Evaluasi harus dilakukan setelah kontrol atau sebelum selesai!";
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormEvaluation([Bind("IdBooking, Evaluation")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.RepairStatus != "EVALUASI")
            {
                TempData["ErrorMessage"] = "Evaluasi harus dilakukan setelah kontrol atau sebelum selesai!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.Evaluation = trsBooking.Evaluation;
            booking.RepairStatus = "SELESAI";
            booking.Progress = 100;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Evaluasi berhasil! Seluruh tahapan sudah selesai!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }               
    }
}
