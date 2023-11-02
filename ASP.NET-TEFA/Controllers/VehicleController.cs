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
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index()
        {
              return _context.MsVehicles != null ? 
                          View(await _context.MsVehicles.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.MsVehicles'  is null.");
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(string id)
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

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdVehicle,Type,Classify,PoliceNumber,Color,Yeart,VehicleOwner,ChassisNumber,MachineNumber")] MsVehicle msVehicle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(msVehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(msVehicle);
        }

        // GET: Vehicle/Edit/5
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

        // POST: Vehicle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdVehicle,Type,Classify,PoliceNumber,Color,Yeart,VehicleOwner,ChassisNumber,MachineNumber")] MsVehicle msVehicle)
        {
            if (id != msVehicle.IdVehicle)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(msVehicle);
                    await _context.SaveChangesAsync();
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
            return View(msVehicle);
        }

        // GET: Vehicle/Delete/5
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

        // POST: Vehicle/Delete/5
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
                _context.MsVehicles.Remove(msVehicle);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MsVehicleExists(string id)
        {
          return (_context.MsVehicles?.Any(e => e.IdVehicle == id)).GetValueOrDefault();
        }
    }
}
