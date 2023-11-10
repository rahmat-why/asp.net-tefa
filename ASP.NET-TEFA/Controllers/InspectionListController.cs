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
    public class InspectionListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InspectionListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InspectionList
        public async Task<IActionResult> Index()
        {
            // Get the IdBooking from query parameters
            string idBooking = HttpContext.Request.Query["IdBooking"];
            var trsBooking = await _context.TrsBookings.FindAsync(idBooking);
            if (trsBooking == null)
            {
                return NotFound();
            }

            // Set ViewBag
            ViewBag.IdBooking = idBooking;

            var trsInspectionLists = _context.TrsInspectionLists
            .Where(t => t.IdBooking ==  idBooking)
            .Include(t => t.IdBookingNavigation)
            .Include(t => t.IdEquipmentNavigation)
            .OrderBy(t => t.IdEquipment)
            .ToListAsync();

            return View(await trsInspectionLists);
        }

        // GET: InspectionList/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TrsInspectionLists == null)
            {
                return NotFound();
            }

            var trsInspectionList = await _context.TrsInspectionLists
                .Include(t => t.IdBookingNavigation)
                .Include(t => t.IdEquipmentNavigation)
                .FirstOrDefaultAsync(m => m.IdInspection == id);
            if (trsInspectionList == null)
            {
                return NotFound();
            }

            return View(trsInspectionList);
        }

        // GET: InspectionList/Create
        public IActionResult Create()
        {
            ViewData["IdBooking"] = new SelectList(_context.TrsBookings, "IdBooking", "IdBooking");
            ViewData["IdEquipment"] = new SelectList(_context.MsEquipments, "IdEquipment", "IdEquipment");
            return View();
        }

        // POST: InspectionList/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection form)
        {
            // Id Booking
            string idBooking = form["IdBooking"].ToString();

            // Lakukan penghapusan data lama
            var existingInspections = _context.TrsInspectionLists.Where(i => i.IdBooking == idBooking);
            _context.TrsInspectionLists.RemoveRange(existingInspections);

            // Simpan data baru berdasarkan input yang diberikan
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("Checklist_"))
                {
                    var idEquipment = key.Replace("Checklist_", "");
                    var checklistValue = form[key].ToString();

                    // Anda juga dapat memeriksa elemen deskripsi dengan nama yang sesuai di sini
                    var descriptionKey = "Description_" + idEquipment;
                    var descriptionValue = form[descriptionKey].ToString();

                    // Konversi nilai checklist ke int (harus sesuai dengan tipe data Checklist di model)
                    int checklist = int.Parse(checklistValue);

                    Random random = new();
                    string idInspection = random.Next(100000, 999999).ToString();
                    var data = new TrsInspectionList
                    {
                        IdInspection = idInspection,
                        IdBooking = idBooking,
                        IdEquipment = idEquipment.ToString(),
                        Checklist = checklist,
                        Description = descriptionValue.ToString()
                    };

                    string dataJson = JsonConvert.SerializeObject(data);

                    // Simpan data ke basis data
                    _context.TrsInspectionLists.Add(data);
                }
            }

            // Simpan perubahan ke basis data
            _context.SaveChanges();

            return RedirectToAction("Index", "Reparation");
        }

        // GET: InspectionList/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TrsInspectionLists == null)
            {
                return NotFound();
            }

            var trsInspectionList = await _context.TrsInspectionLists.FindAsync(id);
            if (trsInspectionList == null)
            {
                return NotFound();
            }
            ViewData["IdBooking"] = new SelectList(_context.TrsBookings, "IdBooking", "IdBooking", trsInspectionList.IdBooking);
            ViewData["IdEquipment"] = new SelectList(_context.MsEquipments, "IdEquipment", "IdEquipment", trsInspectionList.IdEquipment);
            return View(trsInspectionList);
        }

        // POST: InspectionList/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdInspection,IdBooking,IdEquipment,Checklist,Description")] TrsInspectionList trsInspectionList)
        {
            if (id != trsInspectionList.IdInspection)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trsInspectionList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrsInspectionListExists(trsInspectionList.IdInspection))
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
            ViewData["IdBooking"] = new SelectList(_context.TrsBookings, "IdBooking", "IdBooking", trsInspectionList.IdBooking);
            ViewData["IdEquipment"] = new SelectList(_context.MsEquipments, "IdEquipment", "IdEquipment", trsInspectionList.IdEquipment);
            return View(trsInspectionList);
        }

        // GET: InspectionList/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TrsInspectionLists == null)
            {
                return NotFound();
            }

            var trsInspectionList = await _context.TrsInspectionLists
                .Include(t => t.IdBookingNavigation)
                .Include(t => t.IdEquipmentNavigation)
                .FirstOrDefaultAsync(m => m.IdInspection == id);
            if (trsInspectionList == null)
            {
                return NotFound();
            }

            return View(trsInspectionList);
        }

        // POST: InspectionList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TrsInspectionLists == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TrsInspectionLists'  is null.");
            }
            var trsInspectionList = await _context.TrsInspectionLists.FindAsync(id);
            if (trsInspectionList != null)
            {
                _context.TrsInspectionLists.Remove(trsInspectionList);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrsInspectionListExists(string id)
        {
          return (_context.TrsInspectionLists?.Any(e => e.IdInspection == id)).GetValueOrDefault();
        }
    }
}
