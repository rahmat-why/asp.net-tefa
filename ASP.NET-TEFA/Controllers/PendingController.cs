using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_TEFA.Models;
using Newtonsoft.Json;
using System.Drawing;

namespace ASP.NET_TEFA.Controllers
{
    public class PendingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PendingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Menampilkan pending berdasarkan pemesanan tertentu
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> Index(string idBooking)
        {
            // Mencari pemesanan berdasarkan ID
            var booking = await _context.TrsBookings
            .Include(t => t.IdVehicleNavigation)
            .FirstOrDefaultAsync(t => t.IdBooking == idBooking);

            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "INSPECTION LIST" || booking.RepairStatus == "EKSEKUSI" || booking.RepairStatus == "KONTROL"))
            {
                TempData["ErrorMessage"] = "Pending hanya dapat dilakukan saat eksekusi!";
            }

            // Jika pemesanan tidak ditemukan, kembalikan NotFound
            if (booking == null)
            {
                return NotFound();
            }

            // Mengambil daftar pending terkait dengan pemesanan
            var trsPendings = await _context.TrsPendings
            .Where(t => t.IdBooking ==  idBooking)
            .Include(t => t.IdBookingNavigation)
            .Include(t => t.IdUserNavigation)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

            // Menyiapkan data untuk ditampilkan di View
            ViewBag.IdBooking = idBooking;
            ViewBag.Booking = booking;
            ViewBag.Pendings = trsPendings;

            return View();
        }

        // Menambahkan pending jika terdapat temuan
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        [HttpPost]
        public async Task<IActionResult> FormStart([Bind("Reason,IdBooking")] TrsPending trsPending)
        {
            var booking = await _context.TrsBookings.FindAsync(trsPending.IdBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            //validasi untuk mengingatkan agar perencanaan harus terlebih dahulu dilakukan sebelum keputusan
            if (!(booking.RepairStatus == "INSPECTION LIST" || booking.RepairStatus == "EKSEKUSI" || booking.RepairStatus == "KONTROL"))
            {
                TempData["ErrorMessage"] = "Pending hanya dapat dilakukan saat eksekusi!";
                return RedirectToAction("Index", "Reparation", new { idBooking = booking.IdBooking });
            }

            if(trsPending.Reason == null)
            {
                TempData["ErrorMessage"] = "Alasan pending harus diisi!";
                return RedirectToAction("Index", "Pending", new { idBooking = booking.IdBooking });
            }

            // Menghasilkan id pending
            string IdPending = $"PND{_context.TrsPendings.Count() + 1}";

            // Mengambil informasi pelanggan dari sesi
            string userAuthentication = HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            trsPending.IdPending = IdPending;
            trsPending.StartTime = DateTime.Now;
            trsPending.IdUser = user.IdUser;

            // Simpan kedalam transaksi pending
            _context.TrsPendings.Add(trsPending);

            // Simpan kedalam database
            await _context.SaveChangesAsync();

            // Ubah status booking menjadi PENDING
            booking.RepairStatus = "PENDING";
            
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Reparation", new { idBooking = trsPending.IdBooking });
        }

        // Menambahkan pending jika terdapat temuan
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        [HttpPost]
        public async Task<IActionResult> FormFinish(IFormCollection form)
        {
            string IdPending = form["IdPending"];

            var pending = await _context.TrsPendings.FindAsync(IdPending);
            if (pending == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            var booking = await _context.TrsBookings.FindAsync(pending.IdBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            // Menyimpan waktu selesai pending
            pending.FinishTime = DateTime.Now;

            _context.Update(pending);
            await _context.SaveChangesAsync();

            // Ubah status booking menjadi status sesuai dengan progres
            // default jika progress 70 %
            string repair_status = "KONTROL";
            if (booking.Progress == 20)
            {
                repair_status = "INSPECTION LIST";
            }
            else if (booking.Progress == 45)
            {
                repair_status = "EKSEKUSI";
            }
            booking.RepairStatus = repair_status;

            DateTime NewFinishEstimationTime = (DateTime)booking.FinishEstimationTime + ((DateTime)pending.FinishTime-(DateTime)pending.StartTime);
            booking.FinishEstimationTime = NewFinishEstimationTime;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Servis berhasil dilanjutkan dengan estimasi selesai hingga: "+NewFinishEstimationTime.ToString("dd MMMM yyyy - HH:mm")+"!";

            return RedirectToAction("Index", "Reparation", new { idBooking = pending.IdBooking });
        }
    }
}
