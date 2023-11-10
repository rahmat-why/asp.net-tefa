using ASP.NET_TEFA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
