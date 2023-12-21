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

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.RepairStatus == "BATAL")
            {
                TempData["ErrorMessage"] = "This service has been canceled and cannot be resumed!";
            } else if (booking.RepairStatus == "PENDING")
            {
                TempData["ErrorMessage"] = "This service is currently pending!";
            }

            return View(booking);
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> FormStartService(string idBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(idBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "MENUNGGU" || booking.RepairStatus == "PERENCANAAN"))
            {
                TempData["ErrorMessage"] = "Start service must be done when project info";
            }

            // Menyiapkan daftar kendaraan pelanggan untuk dipilih dalam pemesanan
            // Mengambil daftar pengguna dari database yang memiliki password
            var usersWithPassword = await _context.MsUsers
                .Where(user => !string.IsNullOrEmpty(user.Password) && user.Position == "HEAD MECHANIC")
                .ToListAsync();

            ViewData["HeadMechanic"] = new SelectList(usersWithPassword, "IdUser", "FullName");

            return View(booking);
        }

        [HttpPost]
        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> FormStartService([Bind("IdBooking, FinishEstimationTime, HeadMechanic")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }

            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "MENUNGGU" || booking.RepairStatus == "PERENCANAAN"))
            {
                TempData["ErrorMessage"] = "Start service (project info) can be done before planning!";
                return RedirectToAction("History", "Booking");
            }

            //validasi estimasi selesai tidak boleh dibawah saat ini
            if (trsBooking.FinishEstimationTime <= DateTime.Now.AddDays(0))
            {
                TempData["ErrorMessage"] = "Estimated completion should not be below the current time!";
                return RedirectToAction("FormStartService", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            // Mengambil informasi user dari sesi
            string userAuthentication = HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            booking.ServiceAdvisor = user.IdUser;
            booking.FinishEstimationTime = trsBooking.FinishEstimationTime;
            booking.HeadMechanic = trsBooking.HeadMechanic;
            booking.RepairStatus = "PERENCANAAN";

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service successfully started!";

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
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "PERENCANAAN" || booking.RepairStatus == "KEPUTUSAN"))
            {
                TempData["ErrorMessage"] = "Planning must be done before decisions!";
            }

            return View(booking);
        }

        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormPlan([Bind("IdBooking, RepairDescription, ReplacementPart, Price, WorkingCost")] TrsBooking trsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(trsBooking.IdBooking);
            if (booking == null)
            {
                return NotFound();
            }
            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "PERENCANAAN" || booking.RepairStatus == "KEPUTUSAN"))
            {
                TempData["ErrorMessage"] = "Planning should be done after the project info or before the decision!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi deskripsi perbaikan tidak boleh kosong
            if (string.IsNullOrWhiteSpace(trsBooking.RepairDescription))
            {
                TempData["ErrorMessage"] = "Planning must not be empty!";
                return RedirectToAction("FormPlan", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi angka menggunakan regex dan minimum 0
            string priceString = trsBooking.Price.ToString();
            string workingCostString = trsBooking.WorkingCost.ToString();
            Regex regex = new Regex(@"^[0-9]+$");
            if (!regex.IsMatch(priceString) || trsBooking.Price < 0 || !regex.IsMatch(workingCostString) || trsBooking.WorkingCost < 0)
            {
                TempData["ErrorMessage"] = "Bills can only contain numbers, if there are no bills please fill in with a minimum of 0";
                return RedirectToAction("FormPlan", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            // Mengambil informasi user dari sesi
            string userAuthentication = HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            booking.RepairDescription = trsBooking.RepairDescription;
            booking.ReplacementPart = trsBooking.ReplacementPart;
            booking.Price = trsBooking.Price;
            booking.WorkingCost = trsBooking.WorkingCost;
            booking.RepairStatus = "KEPUTUSAN";//mengubah status menjadi Keputusan saat data telah diisi dan disimpan
            booking.Progress = 10;
            booking.HeadMechanic = user.IdUser;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Planning successful! The stage moves on to the decision!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
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
                TempData["ErrorMessage"] = "This service has been canceled and cannot be resumed!";
            }
            else if (!(booking.RepairStatus == "KEPUTUSAN" || booking.RepairStatus == "INSPECTION LIST"))
            {
                TempData["ErrorMessage"] = "Decisions should be made after planning or before execution!";
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
                TempData["ErrorMessage"] = "Decisions should be made after planning or before execution!";
                return RedirectToAction("Index", "Reparation", new { idBooking = idBooking });
            }

            int? decision = int.TryParse(form["decision"], out var parsedValue) ? parsedValue : (int?)null;
            //validasi keputusan tidak boleh kosong
            if (decision == null)
            {
                TempData["ErrorMessage"] = "Decision must be chosen!";
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

            _context.Update(booking);
            await _context.SaveChangesAsync();

            if (repair_status == "BATAL")
            {
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
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Decision successful! The stage continues to the inspection list!";

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
                TempData["ErrorMessage"] = "Execution has been completed on: "+booking.EndRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
            }
            //validasi untuk mengingatkan agar inspection list harus terlebih dahulu dilakukan sebelum selesai eksekusi
            if (!(booking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Finish execution must be done after inspection list!";
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
                TempData["ErrorMessage"] = "Execution has been completed on: "+booking.EndRepairTime.Value.ToString("dd MMMM yyyy - HH:mm");
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi untuk mengingatkan agar inspection list harus terlebih dahulu dilakukan sebelum selesai eksekusi
            if (!(booking.RepairStatus == "EKSEKUSI"))
            {
                TempData["ErrorMessage"] = "Finish execution must be done after inspection list!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.EndRepairTime = DateTime.Now;
            booking.RepairStatus = "KONTROL";
            booking.Progress = 70;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Execution complete! Stages proceed to control!";

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
                TempData["ErrorMessage"] = "Control should be done after the decision or before the evaluation!";
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
                TempData["ErrorMessage"] = "Control should be done after the decision or before the evaluation!";
                return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
            }

            int? control = int.TryParse(form["control"], out var parsedValue) ? parsedValue : (int?)null;
            if (control == null || !control.HasValue)
            {
                TempData["ErrorMessage"] = "Control results must be approved!";
                return RedirectToAction("FormControl", "Reparation", new { idBooking = booking.IdBooking });
            }

            booking.Control = control;
            booking.RepairStatus = "EVALUASI";
            booking.Progress = 90;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Control successful! The stage moves on to evaluation!";

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
                TempData["ErrorMessage"] = "Control successful! The stage moves on to evaluation!";
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
                TempData["ErrorMessage"] = "Evaluation should be done after control or before completion!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            if (string.IsNullOrWhiteSpace(trsBooking.Evaluation))
            {
                TempData["ErrorMessage"] = "Evaluation is required";
                return RedirectToAction("FormEvaluation", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.Evaluation = trsBooking.Evaluation;
            booking.RepairStatus = "SELESAI";
            booking.Progress = 100;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Evaluation successful! All stages have been completed!";

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

        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
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
                TempData["ErrorMessage"] = "Planning should be done after the project info or before the decision!";
                return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi deskripsi perbaikan dan ganti part tidak boleh kosong
            if (string.IsNullOrWhiteSpace(trsBooking.AdditionalReplacementPart) || trsBooking.AdditionalPrice == null)
            {
                TempData["ErrorMessage"] = "Additional replacement part and the price cannot be empty!";
                return RedirectToAction("FormSpecialHandling", "Reparation", new { idBooking = trsBooking.IdBooking });
            }
            //validasi angka menggunakan regex dan minimum 0
            string priceString = trsBooking.AdditionalPrice.ToString();
            Regex regex = new Regex(@"^[0-9]+$");
            if (!regex.IsMatch(priceString) || trsBooking.Price < 0)
            {
                TempData["ErrorMessage"] = "Price can only contain a minimum of 0";
                return RedirectToAction("FormSpecialHandling", "Reparation", new { idBooking = trsBooking.IdBooking });
            }

            booking.AdditionalReplacementPart = trsBooking.AdditionalReplacementPart;
            booking.AdditionalPrice = trsBooking.AdditionalPrice;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Special problem saved successfully!";

            return RedirectToAction("Index", "Reparation", new { idBooking = trsBooking.IdBooking });
        }
    }
}
