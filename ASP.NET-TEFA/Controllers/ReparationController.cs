using ASP.NET_TEFA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace ASP.NET_TEFA.Controllers
{
    [AuthorizedUser]
    public class ReparationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReparationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            
            string id_booking = "BKN1";
            var applicationDbContext = _context.TrsBookings.Where(c => c.IdBooking == id_booking).Include(t => t.IdVehicleNavigation);
            return View(await applicationDbContext.ToListAsync());
        }



        // GET: Booking/Edit/5
        public async Task<IActionResult> Plan()
        {
            string idBooking = HttpContext.Request.Query["IdBooking"];

            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);
            if (trsBooking == null)
            {
                return NotFound();
            }
            ViewBag.IdBooking = idBooking;

            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Plan([Bind("IdBooking,RepairDescription,ReplacementPart")] TrsBooking TrsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(TrsBooking.IdBooking);
            String decision = "KEPUTUSAN";

            booking.RepairDescription = TrsBooking.RepairDescription;
            booking.ReplacementPart = TrsBooking.ReplacementPart;
            booking.RepairStatus = decision;

            _context.Update(booking);
             await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
            
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Decision()
        {
            string idBooking = HttpContext.Request.Query["IdBooking"];

            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);
            if (trsBooking == null)
            {
                return NotFound();
            }
            ViewBag.IdBooking = idBooking;

            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decision([Bind("IdBooking,Price,RepairDescription,ReplacementPart")] TrsBooking TrsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(TrsBooking.IdBooking);
            String decision = "INSPECTION LIST";

            booking.RepairDescription = TrsBooking.RepairDescription;
            booking.ReplacementPart = TrsBooking.ReplacementPart;
            booking.Price = TrsBooking.Price;
            booking.RepairStatus = decision;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Kontrol()
        {
            string idBooking = HttpContext.Request.Query["IdBooking"];

            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);
            if (trsBooking == null)
            {
                return NotFound();
            }
            ViewBag.IdBooking = idBooking;

            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kontrol([Bind("IdBooking,Control")] TrsBooking TrsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(TrsBooking.IdBooking);
            String decision = "EVALUASI";

            booking.Control = TrsBooking.Control;
            booking.RepairStatus = decision;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }


        // GET: Booking/Edit/5
        public async Task<IActionResult> Evaluasi()
        {
            string idBooking = HttpContext.Request.Query["IdBooking"];

            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);
            if (trsBooking == null)
            {
                return NotFound();
            }
            ViewBag.IdBooking = idBooking;

            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles, "IdVehicle", "IdVehicle", trsBooking.IdVehicle);
            return View(trsBooking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluasi([Bind("IdBooking,Evaluation")] TrsBooking TrsBooking)
        {
            var booking = await _context.TrsBookings.FindAsync(TrsBooking.IdBooking);
            String decision = "SELESAI";

            booking.Evaluation = TrsBooking.Evaluation;
            booking.RepairStatus = decision;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
