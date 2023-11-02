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
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TrsBookings.Include(t => t.IdCustomerNavigation).Include(t => t.IdVehicleNavigation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TrsBookings == null)
            {
                return NotFound();
            }

            var trsBooking = await _context.TrsBookings
                .Include(t => t.IdCustomerNavigation)
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
            ViewData["IdCustomer"] = new SelectList(_context.MsCustomers, "IdCustomer", "IdCustomer");
            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle");
            return View();
        }

        // POST: Booking/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBooking,OrderDate,IdVehicle,Odometer,Complaint,IdCustomer")] TrsBooking trsBooking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trsBooking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCustomer"] = new SelectList(_context.MsCustomers, "IdCustomer", "IdCustomer", trsBooking.IdCustomer);
            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
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
            ViewData["IdCustomer"] = new SelectList(_context.MsCustomers, "IdCustomer", "IdCustomer", trsBooking.IdCustomer);
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
            ViewData["IdCustomer"] = new SelectList(_context.MsCustomers, "IdCustomer", "IdCustomer", trsBooking.IdCustomer);
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
                .Include(t => t.IdCustomerNavigation)
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
