using ASP.NET_TEFA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ASP.NET_TEFA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AuthorizedCustomer]
        public IActionResult Index()
        {
            string authentication = HttpContext.Session.GetString("authentication");
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            return View(customer);
        }

        [AuthorizedCustomer]
        public IActionResult Privacy()
        {
            return View();
        }

        [AuthorizedCustomer]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}