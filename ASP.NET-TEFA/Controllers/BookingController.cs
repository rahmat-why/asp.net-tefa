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
using System.Globalization;

namespace ASP.NET_TEFA.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private object msVehicle;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AuthorizedCustomer]
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

        [AuthorizedUser]
        public async Task<IActionResult> Report()
        {
            IQueryable<TrsBooking> query = _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .ThenInclude(v => v.IdCustomerNavigation)
                .Where(t => t.RepairStatus == "SELESAI");

            string monthString = HttpContext.Request.Query["month"];

            if (DateTime.TryParseExact(monthString, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth))
            {
                int month = parsedMonth.Month;
                Console.WriteLine(month);

                query = query.Where(t => t.OrderDate != null && t.OrderDate.Value.Month == month);
            }

            var reportBooking = await query.OrderBy(t => t.OrderDate).ToListAsync();

            int countTefa = reportBooking.Count(t => t.RepairMethod == "TEFA");
            int countService = reportBooking.Count(t => t.RepairMethod == "SERVICE");

            TempData["month"] = monthString;
            TempData["count_tefa"] = countTefa;
            TempData["count_service"] = countService;

            return View(reportBooking);
        }

        [AuthorizedCustomer]
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

        [AuthorizedCustomer]
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBooking,OrderDate,IdVehicle,Odometer,Complaint,IdCustomer")] TrsBooking trsBooking)
        {
            // Check if OrderDate is in the past
            if (trsBooking.OrderDate < DateTime.Now.AddDays(1))
            {
                TempData["ErrorMessage"] = "Tanggal yang valid minimum H+1";
                return View();
            }

            // Generate id booking
            string IdBooking = $"BKN{_context.TrsBookings.Count() + 1}";

            // Assign id booking
            trsBooking.IdBooking = IdBooking;

            _context.Add(trsBooking);
            await _context.SaveChangesAsync();

            // Send alert to view
            TempData["SuccessMessage"] = "Booking berhasil!";

            return RedirectToAction("Index", "Booking");
        }

        private bool TrsBookingExists(string id)
        {
          return (_context.TrsBookings?.Any(e => e.IdBooking == id)).GetValueOrDefault();
        }


    }
}
