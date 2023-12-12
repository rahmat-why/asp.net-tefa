using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_TEFA.Models;

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

            // Jika pemesanan tidak ditemukan, kembalikan NotFound
            if (booking == null)
            {
                return NotFound();
            }

            // Mengambil daftar pending terkait dengan pemesanan
            var trsPendings = _context.TrsPendings
            .Where(t => t.IdBooking ==  idBooking)
            .Include(t => t.IdBookingNavigation)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

            // Menyiapkan data untuk ditampilkan di View
            ViewBag.IdBooking = idBooking;
            ViewBag.booking = booking;

            return View(await trsPendings);
        }

        // Menambahkan pending jika terdapat temuan
        [AuthorizedUser("HEAD MECHANIC")]
        [HttpPost]
        public async Task<IActionResult> FormStart(IFormCollection form)
        {
            // Ambil data dari form
            string IdBooking = form["IdBooking"];
            string reason = form["reason"];

            var booking = await _context.TrsBookings.FindAsync(IdBooking);
            if (booking == null)
            {
                return RedirectToAction("NotFound", "Authentication");
            }

            // Menghasilkan id pending
            string IdPending = $"PND{_context.TrsPendings.Count() + 1}";

            // Tambah objek pending
            TrsPending newPending = new TrsPending
            {
                IdPending = IdPending,
                IdBooking = IdBooking,
                Reason = reason,
                StartTime = DateTime.Now
            };

            // Simpan kedalam transaksi pending
            _context.TrsPendings.Add(newPending);

            // Simpan kedalam database
            await _context.SaveChangesAsync();

            // Ubah status booking menjadi PENDING
            booking.RepairStatus = "PENDING";
            
            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Servis berhasil dipending sementara!";

            return RedirectToAction("Index", "Reparation", new { idBooking = IdBooking });
        }

        // Menambahkan pending jika terdapat temuan
        [AuthorizedUser("HEAD MECHANIC")]
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
