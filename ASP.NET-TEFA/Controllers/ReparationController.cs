using ASP.NET_TEFA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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

            if(booking.RepairStatus == "BATAL")
            {
                TempData["ErrorMessage"] = "Servis ini telah dibatalkan dan tidak dapat dilanjutkan lagi!";
            }else if(booking.RepairStatus == "PENDING")
            {
                TempData["ErrorMessage"] = "Servis ini sedang dipending!";
            }

            return View(booking);
        }

        [HttpPost]
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormStartService(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return NotFound();
            }

            // Mengambil informasi user dari sesi
            string userAuthentication = HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            booking.ServiceAdvisor = user.IdUser;
            booking.RepairStatus = "PERENCANAAN";

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Servis berhasil dimulai!";

            return RedirectToAction("Index", "Reparation", new { idBooking = idBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormPlan(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "PERENCANAAN" || booking.RepairStatus == "KEPUTUSAN"))
            {
                TempData["ErrorMessage"] = "Perencanaan harus dilakukan sebelum keputusan!";
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormPlan([Bind("IdBooking, RepairDescription, ReplacementPart, Price, FinishEstimationTime")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "PERENCANAAN" || booking.RepairStatus == "KEPUTUSAN"))
            {
                TempData["ErrorMessage"] = "Perencanaan harus dilakukan setelah info proyek atau sebelum keputusan!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi deskripsi perbaikan dan ganti part tidak boleh kosong
            if (string.IsNullOrWhiteSpace(trsBooking.RepairDescription) || string.IsNullOrWhiteSpace(trsBooking.ReplacementPart) || trsBooking.FinishEstimationTime == null)
            {
                TempData["ErrorMessage"] = "Deskripsi perbaikan, ganti part, dan tagihan tidak boleh kosong! Isi dengan 0 jika ingin mengosongkan.";
                return RedirectToAction("FormPlan", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi angka menggunakan regex dan minimum 0
            string priceString = trsBooking.Price.ToString();
            Regex regex = new Regex(@"^[0-9]+$");
            if (!regex.IsMatch(priceString) || trsBooking.Price < 0)
            {
                TempData["ErrorMessage"] = "Tagihan hanya boleh berisi angka, jika tidak ada tagihan silahkan isi dengan 0 dan harus minimum 0";
                return RedirectToAction("FormPlan", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi estimasi selesai tidak boleh dibawah saat ini
            if (trsBooking.FinishEstimationTime <= DateTime.Now.AddDays(0))
            {
                TempData["ErrorMessage"] = "Estimasi selesai tidak boleh dibawah waktu saat ini!";
                return RedirectToAction("FormPlan", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            // Mengambil informasi user dari sesi
            string userAuthentication = HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            booking.RepairDescription = trsBooking.RepairDescription;
            booking.ReplacementPart = trsBooking.ReplacementPart;
            booking.Price = trsBooking.Price;
            booking.FinishEstimationTime = trsBooking.FinishEstimationTime;
            booking.RepairStatus = "KEPUTUSAN";//mengubah status menjadi Keputusan saat data telah diisi dan disimpan
            booking.Progress = 10;
            booking.HeadMechanic = user.IdUser;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Perencanaan berhasil! Tahapan berlanjut ke keputusan!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> FormDecision(string idBooking)
        {
            var booking = await _context.TrsBookings
            .Include(t => t.IdVehicleNavigation)
            .ThenInclude(v => v.IdCustomerNavigation)
            .FirstOrDefaultAsync(c => c.IdBooking == idBooking);

            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }
            //validasi peringatan harus mengisi keputusan sebelum eksekusi
            if(booking.RepairStatus == "BATAL")
            {
                TempData["ErrorMessage"] = "Servis ini telah dibatalkan dan tidak dapat dilanjutkan lagi!";
            }
            else if (!(booking.RepairStatus == "KEPUTUSAN" || booking.RepairStatus == "INSPECTION LIST"))
            {
                TempData["ErrorMessage"] = "Keputusan harus dilakukan setelah perencanaan atau sebelum eksekusi!";
            }

            return View(booking);
        }

        [AuthorizedUser("SERVICE ADVISOR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormDecision(IFormCollection form)
        {
            string idBooking = form["IdBooking"].ToString();
            var booking = await _context.TrsBookings
            .Include(t => t.IdVehicleNavigation)
            .FirstOrDefaultAsync(t => t.IdBooking == idBooking);

            if (booking == null)
            {
                return NotFound();
            }
            //validasi untuk mengingatkan agar keputusan harus terlebih dahulu dilakukan sebelum inspection list
            if (!(booking.RepairStatus == "KEPUTUSAN" || booking.RepairStatus == "INSPECTION LIST"))
            {
                TempData["ErrorMessage"] = "Keputusan harus dilakukan setelah perencanaan atau sebelum eksekusi!";
                return RedirectToAction("Index", "Reparation", new { idBooking = idBooking });
            }

            int? decision = int.TryParse(form["decision"], out var parsedValue) ? parsedValue : (int?)null;
            //validasi keputusan tidak boleh kosong
            if (decision == null)
            {
                TempData["ErrorMessage"] = "Persetujuan harus dipilih!";
                return RedirectToAction("FormDecision", "Reparation", new { idBooking = idBooking });
            }

            string repair_status;
            if(decision == 1)
            {
                repair_status = "INSPECTION LIST";
            }
            else
            {
                repair_status = "BATAL";
            }

            booking.Decision = decision;
            booking.RepairStatus = repair_status;
            booking.Progress = 20;

            if(repair_status == "BATAL")
            {
                TempData["SuccessMessage"] = "Servis berhasil dibatalkan!";
                return RedirectToAction("Index", "Reparation", new { idBooking = idBooking });
            }

            // menyimpan data mentah inspection list dari tabel equipment jika kendaraan mobil
            if (booking.IdVehicleNavigation.Classify == "MOBIL")
            {
                // Lakukan penghapusan data lama
                var existingInspections = _context.TrsInspectionLists.Where(i => i.IdBooking == idBooking);
                _context.TrsInspectionLists.RemoveRange(existingInspections);

                var equipment = await _context.MsEquipments.Where(t => t.IsActive == 1).ToListAsync();
                for (int i = 0; i < equipment.Count; i++)
                {
                    Random random = new();
                    string idInspection = random.Next(100000, 999999).ToString();
                    var data = new TrsInspectionList
                    {
                        IdInspection = idInspection,
                        IdBooking = idBooking,
                        IdEquipment = equipment[i].IdEquipment,
                        Checklist = 1,
                        Description = null
                    };

                    _context.TrsInspectionLists.Add(data);
                }
            }

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Keputusan berhasil! Tahapan berlanjut ke inspection list!";

            return RedirectToAction("Index", "Reparation", new { idBooking = idBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormFinishExecution(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }
            // validasi jika eksekusi sudah diselesaikan
            if (booking.EndRepairTime != null)
            {
                TempData["ErrorMessage"] = "Eksekusi sudah diselesaikan pada: "+booking.EndRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
            }
            //validasi untuk mengingatkan agar inspection list harus terlebih dahulu dilakukan sebelum selesai eksekusi
            if (!(booking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Selesai eksekusi harus dilakukan setelah inspection list!";
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
            // validasi jika eksekusi sudah diselesaikan
            if (booking.EndRepairTime != null)
            {
                TempData["ErrorMessage"] = "Eksekusi sudah diselesaikan pada: "+booking.EndRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi untuk mengingatkan agar inspection list harus terlebih dahulu dilakukan sebelum selesai eksekusi
            if (!(booking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Selesai eksekusi harus dilakukan setelah inspection list!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.EndRepairTime = DateTime.Now;
            booking.RepairStatus = "KONTROL";
            booking.Progress = 70;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Eksekusi selesai! Tahapan berlanjut ke kontrol!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR")]
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

            int? control = int.TryParse(form["control"], out var parsedValue) ? parsedValue : (int?)null;
            if (control == null || !control.HasValue)
            {
                TempData["ErrorMessage"] = "Hasil kontrol harus disetujui!";
                return RedirectToAction("FormControl", "Reparation", new { idBooking = booking.IdBooking });
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
            if (string.IsNullOrWhiteSpace(trsBooking.Evaluation))
            {
                TempData["ErrorMessage"] = "Evaluasi harus diisi, gunakan (-) jika tidak ada evaluasi";
                return RedirectToAction("FormEvaluation", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.Evaluation = trsBooking.Evaluation;
            booking.RepairStatus = "SELESAI";
            booking.Progress = 100;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Evaluasi berhasil! Seluruh tahapan sudah selesai!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormSpecialHandling(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan hanya saat pending
            if (!(booking.RepairStatus == "PENDING"))
            {
                TempData["ErrorMessage"] = "Temuan dapat dilakukan saat pending!";
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormSpecialHandling([Bind("IdBooking, AdditionalReplacementPart, AdditionalPrice")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan hanya saat pending
            if (!(booking.RepairStatus == "PENDING"))
            {
                TempData["ErrorMessage"] = "Perencanaan harus dilakukan setelah info proyek atau sebelum keputusan!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi deskripsi perbaikan dan ganti part tidak boleh kosong
            if (string.IsNullOrWhiteSpace(trsBooking.AdditionalReplacementPart) || trsBooking.AdditionalPrice == null)
            {
                TempData["ErrorMessage"] = "Tambahan ganti part, dan tagihan tidak boleh kosong! Isi dengan 0 jika ingin mengosongkan.";
                return RedirectToAction("FormSpecialHandling", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi angka menggunakan regex dan minimum 0
            string priceString = trsBooking.AdditionalPrice.ToString();
            Regex regex = new Regex(@"^[0-9]+$");
            if (!regex.IsMatch(priceString) || trsBooking.Price < 0)
            {
                TempData["ErrorMessage"] = "Tagihan hanya boleh berisi angka, jika tidak ada tagihan silahkan isi dengan 0 dan harus minimum 0";
                return RedirectToAction("FormSpecialHandling", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.AdditionalReplacementPart = trsBooking.AdditionalReplacementPart;
            booking.AdditionalPrice = trsBooking.AdditionalPrice;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Temuan berhasil disimpan!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }
    }
}
