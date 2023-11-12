using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP.NET_TEFA.Models;
using Newtonsoft.Json;

namespace ASP.NET_TEFA.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
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
                var applicationDbContext = _context.MsVehicles
                    .Include(t => t.IdCustomerNavigation)
                    .Where(t => t.IdCustomerNavigation.IdCustomer == customer.IdCustomer).Where(t => t.Classify != "NONAKTIF");

                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [AuthorizedCustomer]
        public IActionResult Create()
        {
            return View();
        }

        [AuthorizedCustomer]
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdVehicle,Type,Classify,PoliceNumber,Color,Year,VehicleOwner,ChassisNumber,MachineNumber")] MsVehicle msVehicle)
        {
            string authentication = HttpContext.Session.GetString("authentication");
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            if (customer != null)
            {
                string Idvehicle = $"VC{_context.MsVehicles.Count() + 1}";
                // Generate id customer
                msVehicle.IdCustomer = customer.IdCustomer;
                // Assign id customer
                msVehicle.IdVehicle = Idvehicle;
                _context.Add(msVehicle);
                await _context.SaveChangesAsync();
                // Send alert to view
                TempData["SuccessMessage"] = "Kendaraan berhasil disimpan!";

                return RedirectToAction(nameof(Index));
            }

            return View(msVehicle);
        }

        [AuthorizedCustomer]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.MsVehicles == null)
            {
                return NotFound();
            }

            var msVehicle = await _context.MsVehicles.FindAsync(id);
            if (msVehicle == null)
            {
                return NotFound();
            }
            return View(msVehicle);
        }

        [AuthorizedCustomer]
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdVehicle,Type,Classify,PoliceNumber,Color,Year,VehicleOwner,ChassisNumber,MachineNumber,IdCustomer")] MsVehicle msVehicle)
        {
            if (id != msVehicle.IdVehicle)
            {
                return NotFound();
            }

            try
            {
                Console.WriteLine(msVehicle.IdCustomer);
                _context.Update(msVehicle);
                await _context.SaveChangesAsync();
                // Send alert to view
                TempData["SuccessMessage"] = "Kendaraan berhasil diperbaharui!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MsVehicleExists(msVehicle.IdVehicle))
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

        [AuthorizedCustomer]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.MsVehicles == null)
            {
                return NotFound();
            }

            var msVehicle = await _context.MsVehicles
                .FirstOrDefaultAsync(m => m.IdVehicle == id);
            if (msVehicle == null)
            {
                return NotFound();
            }

            return View(msVehicle);
        }

        [AuthorizedCustomer]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.MsVehicles == null)
            {
                return Problem("Entity set 'ApplicationDbContext.MsVehicles'  is null.");
            }
            var msVehicle = await _context.MsVehicles.FindAsync(id);
            if (msVehicle != null)
            {
                msVehicle.Classify = "NONAKTIF";
                _context.MsVehicles.Update(msVehicle);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MsVehicleExists(string id)
        {
          return (_context.MsVehicles?.Any(e => e.IdVehicle == id)).GetValueOrDefault();
        }

        [AuthorizedUser]
        public async Task<IActionResult> History(string id)
        {
            if (id == null || _context.MsVehicles == null)
            {
                return NotFound();
            }

            var vehicle = await _context.MsVehicles
                .Include(v => v.TrsBookings) // Include the TrsBooking navigation property
                .FirstOrDefaultAsync(m => m.IdVehicle == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }
    }
}
