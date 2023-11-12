using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_TEFA.Models;
using Newtonsoft.Json;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Controllers
{
    [AuthorizedUser]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private object msVehicle;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index()
        {
            string authentication = HttpContext.Session.GetString("authentication");
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            if (customer != null)
            {
                var applicationDbContext = _context.TrsBookings
                    .Include(t => t.IdVehicleNavigation)
                    .Where(t => t.IdVehicleNavigation.IdCustomer == customer.IdCustomer)
                    .OrderBy(t => t.OrderDate);//kode untuk menampilakn hanya yang login dan di filter tanggal terbaru

                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> History()
        {
            var applicationDbContext = _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .OrderBy(t => t.OrderDate);//kode untuk menampilakn hanya yang login dan di filter tanggal terbaru

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> FormMethod()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FormMethod(string id, string RepairMethod, DateTime? EndRepairTime)
        {
            if (id == null)
            {
                return NotFound();
            }

            var TrsBookings = await _context.TrsBookings.FindAsync(id);
            if (TrsBookings == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(RepairMethod))
            {
                TrsBookings.RepairMethod = RepairMethod;
            }

            if (EndRepairTime != null)
            {
                TrsBookings.EndRepairTime = EndRepairTime;
            }
            try
            {
                // Menyimpan perubahan ke database
                _context.Update(TrsBookings);
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {
                Console.WriteLine(123);
            }

            // update trs booking, metode & estimasi

            return RedirectToAction("History");
        }

            // GET: Booking/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TrsBookings == null)
            {
                return NotFound();
            }

            var trsBooking = await _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .FirstOrDefaultAsync(m => m.IdBooking == id);
            if (trsBooking == null)
            {
                return NotFound();
            }

            return View(trsBooking);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            string authentication = HttpContext.Session.GetString("authentication");
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            if (customer != null)
            {
                ViewData["IdVehicle"] = new SelectList(_context.MsVehicles.Where(c => c.IdCustomer == customer.IdCustomer), "IdVehicle", "Type");//diubah
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Booking/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBooking,OrderDate,IdVehicle,Odometer,Complaint,IdCustomer")] TrsBooking trsBooking)
        {
            // Generate id customer
            string IdBooking = $"BK{_context.TrsBookings.Count() + 1}";

            // Assign id customer
            trsBooking.IdBooking = IdBooking;
            _context.Add(trsBooking);
            await _context.SaveChangesAsync();

            // Send alert to view
            TempData["SuccessMessage"] = "Booking berhasil!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TrsBookings == null)
            {
                return NotFound();
            }

            var trsBooking = await _context.TrsBookings.FindAsync(id);
            if (trsBooking == null)
            {
                return NotFound();
            }

            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
        }

        // POST: Booking/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdBooking,OrderDate,IdVehicle,Odometer,Complaint,IdCustomer")] TrsBooking trsBooking)
        {
            if (id != trsBooking.IdBooking)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trsBooking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrsBookingExists(trsBooking.IdBooking))
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

            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TrsBookings == null)
            {
                return NotFound();
            }

            var trsBooking = await _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .FirstOrDefaultAsync(m => m.IdBooking == id);
            if (trsBooking == null)
            {
                return NotFound();
            }

            return View(trsBooking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TrsBookings == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TrsBookings'  is null.");
            }
            var trsBooking = await _context.TrsBookings.FindAsync(id);
            if (trsBooking != null)
            {
                _context.TrsBookings.Remove(trsBooking);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrsBookingExists(string id)
        {
          return (_context.TrsBookings?.Any(e => e.IdBooking == id)).GetValueOrDefault();
        }


    }
}
