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
using OfficeOpenXml.Table;
using OfficeOpenXml;

namespace ASP.NET_TEFA.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        readonly IReporting _IReporting;

        public BookingController(ApplicationDbContext context, IReporting iReporting)
        {
            _context = context;
            _IReporting = iReporting;
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
                    .OrderBy(t => t.OrderDate);

                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Servis()
        {
            var runningServices = await _context.TrsBookings
            .Include(t => t.IdVehicleNavigation)
            .ThenInclude(v => v.IdCustomerNavigation)
            .Where(t => t.StartRepairTime != null && t.RepairStatus != "SELESAI")
            .ToListAsync();

            return View(runningServices);
        }

        public async Task<IActionResult> History()
        {
            var applicationDbContext = _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                    .ThenInclude(v => v.IdCustomerNavigation)
                .OrderBy(t => t.OrderDate)
                .ToList();

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                // other settings if needed
            };

            string json = JsonConvert.SerializeObject(applicationDbContext, jsonSettings);
            Console.WriteLine(json);

            return View(applicationDbContext);
        }


        [AuthorizedUser]
        public async Task<IActionResult> Report()
        {
            IQueryable<TrsBooking> query = _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .ThenInclude(v => v.IdCustomerNavigation)
                .Where(t => t.RepairStatus == "SELESAI");

            string monthString = HttpContext.Request.Query["month"];
            if (string.IsNullOrEmpty(monthString))
            {
                monthString = DateTime.Now.ToString("yyyy-MM");
            }

            if (DateTime.TryParseExact(monthString, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth))
            {
                int month = parsedMonth.Month;

                query = query.Where(t => t.OrderDate != null && t.OrderDate.Value.Month == month);
            }

            var reportBooking = await query.OrderBy(t => t.OrderDate).ToListAsync();

            int countTefa = reportBooking.Count(t => t.RepairMethod == "TEFA");
            int countService = reportBooking.Count(t => t.RepairMethod == "SERVICE");

            ViewBag.month = monthString;
            TempData["count_tefa"] = countTefa;
            TempData["count_service"] = countService;

            return View(reportBooking);
        }

        [AuthorizedUser]
        public IActionResult Export()
        {
            string monthString = HttpContext.Request.Query["month"];
            string reportname = $"BOOKING_{Guid.NewGuid():N}.xlsx";
            var list = _IReporting.GetReportTrsBooking(monthString);

            if (list.Count == 0)
            {
                ViewBag.month = monthString;
                TempData["ErrorMessage"] = "Data pada bulan yang dipilih tidak ada!";
                return RedirectToAction("Report", "Booking");
            }

            var exportbytes = ExporttoExcel<TrsBooking>(list, reportname);
            return File(exportbytes, "applicatio/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname);
        }

        private byte[] ExporttoExcel<T>(List<T> table, string filename)
        {
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
            ws.Cells["A1"].LoadFromCollection(table, true, TableStyles.Light1);
            return pack.GetAsByteArray();
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
