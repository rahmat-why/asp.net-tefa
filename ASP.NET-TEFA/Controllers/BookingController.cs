using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
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

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

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

        // Menampilkan halaman indeks untuk pemesanan oleh pelanggan yang terotorisasi
        [AuthorizedCustomer]
        public async Task<IActionResult> Index()
        {
            // Mengambil data autentikasi pelanggan dari sesi
            string authentication = HttpContext.Session.GetString("authentication");

            // Deserialisasi data autentikasi pelanggan menjadi objek MsCustomer
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            // Memeriksa apakah pelanggan terautentikasi
            if (customer != null)
            {
                // Mengambil data pemesanan yang mencakup kendaraan dan pelanggan
                var applicationDbContext = _context.TrsBookings
                    .Include(t => t.IdVehicleNavigation)
                    .Where(t => t.IdVehicleNavigation.IdCustomer == customer.IdCustomer)
                    .OrderBy(t => t.OrderDate);

                // Menampilkan daftar pemesanan ke view
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                // Jika pelanggan tidak terautentikasi, kembalikan ke halaman utama
                return RedirectToAction(nameof(Index));
            }
        }

        // Menampilkan halaman kemajuan pemesanan untuk pengguna yang terotorisasi
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> Progress()
        {
            // Mengambil data pemesanan yang sedang berlangsung, termasuk kendaraan dan pelanggan terkait
            var runningServices = await _context.TrsBookings
            .Include(t => t.IdVehicleNavigation)
            .ThenInclude(v => v.IdCustomerNavigation)
            .Where(t => t.RepairMethod != null && t.RepairStatus != "SELESAI")
            .OrderByDescending(x => x.Progress)
            .ToListAsync();

            // Menampilkan daftar pemesanan yang sedang berlangsung ke view
            return View(runningServices);
        }

        // Menampilkan halaman histori pemesanan untuk pengguna yang terotorisasi
        [AuthorizedUser("SERVICE ADVISOR", "HEAD MECHANIC")]
        public async Task<IActionResult> History()
        {
            // Mengambil informasi autentikasi pengguna dari sesi
            string userAuthentication = HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            // Membuat kueri untuk mengambil data pemesanan yang belum selesai, termasuk kendaraan dan pelanggan terkait
            var query = _context.TrsBookings
            .Include(t => t.IdVehicleNavigation)
            .ThenInclude(v => v.IdCustomerNavigation)
            .Where(t => t.RepairStatus != "SELESAI");

            // Jika pengguna bukan SERVICE ADVISOR, hanya tampilkan pemesanan dengan metode perbaikan terisi
            // Diurutkan berdasarkan order date terbaru
            if (user.Position != "SERVICE ADVISOR")
            {
                query = query.Where(t => t.RepairMethod != null).OrderBy(t => t.OrderDate);
            }

            // Mengambil data pemesanan berdasarkan kueri dan mengurutkannya berdasarkan tanggal pesan
            var applicationDbContext = await query
                .OrderBy(t => t.OrderDate)
                .ToListAsync();

            // Menampilkan daftar pemesanan ke view
            return View(applicationDbContext);
        }

        // Menampilkan halaman laporan pemesanan untuk pengguna yang terotorisasi sebagai SERVICE ADVISOR
        [AuthorizedUser("SERVICE ADVISOR")]
        public async Task<IActionResult> Report()
        {
            // Membuat kueri untuk mengambil data pemesanan yang telah selesai, termasuk kendaraan dan pelanggan terkait
            IQueryable<TrsBooking> query = _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .ThenInclude(v => v.IdCustomerNavigation)
                .Where(t => t.RepairStatus == "SELESAI" || t.RepairStatus == "BATAL");

            // Mengambil nilai bulan dari parameter query atau menggunakan bulan saat ini jika tidak ada
            string monthString = HttpContext.Request.Query["month"];
            if (string.IsNullOrEmpty(monthString))
            {
                monthString = DateTime.Now.ToString("yyyy-MM");
            }

            // Memeriksa apakah nilai bulan yang diberikan dapat di-parse menjadi objek DateTime
            if (DateTime.TryParseExact(monthString, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth))
            {
                // Jika berhasil di-parse, ambil bulan dari objek DateTime dan filter pemesanan berdasarkan bulan tersebut
                int month = parsedMonth.Month;

                query = query.Where(t => t.OrderDate != null && t.OrderDate.Value.Month == month);
            }

            // Mengambil data pemesanan berdasarkan kueri dan mengurutkannya berdasarkan tanggal pesan
            var reportBooking = await query.OrderBy(t => t.OrderDate).ToListAsync();

            // Menghitung jumlah pemesanan dengan metode perbaikan TEFA dan SERVICE
            int countTefa = reportBooking.Count(t => t.RepairMethod == "TEFA");
            int countFastTrack = reportBooking.Count(t => t.RepairMethod == "FAST TRACK");
            int countCancel = reportBooking.Count(t => t.RepairStatus == "BATAL");

            // Menetapkan nilai bulan dan jumlah pemesanan ke ViewBag dan TempData untuk digunakan di view
            ViewBag.month = monthString;
            TempData["count_tefa"] = countTefa;
            TempData["count_fast_track"] = countFastTrack;
            TempData["count_cancel"] = countCancel;

            // Menampilkan data pemesanan ke view
            return View(reportBooking);
        }

        // Menampilkan halaman ekspor laporan pemesanan untuk pengguna yang terotorisasi sebagai SERVICE ADVISOR
        [AuthorizedUser("SERVICE ADVISOR")]
        public IActionResult Export()
        {
            // Mengambil nilai bulan dari parameter query
            string monthString = HttpContext.Request.Query["month"];

            // Membuat nama file laporan dengan format "BOOKING_{Guid}.xlsx"
            string reportname = $"BOOKING_{Guid.NewGuid():N}.xlsx";

            // Mengambil data pemesanan untuk bulan yang dipilih menggunakan IReporting
            var list = _IReporting.GetReportTrsBooking(monthString);

            // Memeriksa apakah ada data pemesanan untuk bulan yang dipilih
            if (list.Count == 0)
            {
                ViewBag.month = monthString;
                TempData["ErrorMessage"] = "Data pada bulan yang dipilih tidak ada!";
                return RedirectToAction("Report", "Booking");
            }

            // Ekspor data pemesanan ke file Excel dan mendapatkan byte array hasil ekspor
            var exportbytes = ExporttoExcel(list, reportname);

            // Mengembalikan file Excel sebagai respons
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname);
        }

        // Metode untuk mengekspor data pemesanan ke file Excel dengan format yang ditentukan
        private byte[] ExporttoExcel(List<TrsBooking> data, string filename)
        {
            // Membuat objek ExcelPackage untuk menyimpan data Excel
            using (ExcelPackage pack = new ExcelPackage())
            {
                // Menambahkan worksheet ke ExcelPackage
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);

                // Menambahkan baris header
                ws.Cells["A1"].Value = "ID Booking";
                ws.Cells["B1"].Value = "Nama Pelanggan";
                ws.Cells["C1"].Value = "Tanggal Booking";
                ws.Cells["D1"].Value = "Tipe Kendaraan";
                ws.Cells["E1"].Value = "No. Polisi";
                ws.Cells["F1"].Value = "Odometer (km)";
                ws.Cells["G1"].Value = "Keluhan";
                ws.Cells["H1"].Value = "Mulai Servis";
                ws.Cells["I1"].Value = "Selesai Servis";
                ws.Cells["J1"].Value = "Estimasi Selesai";
                ws.Cells["K1"].Value = "Deskripsi Perbaikan";
                ws.Cells["L1"].Value = "Deskripsi Ganti Part";
                ws.Cells["M1"].Value = "Tagihan";
                ws.Cells["N1"].Value = "Evaluasi";
                ws.Cells["O1"].Value = "Metode Servis";
                ws.Cells["P1"].Value = "Tambahan Ganti Part";
                ws.Cells["Q1"].Value = "Tambahan Tagihan";
                ws.Cells["R1"].Value = "Keputusan";

                // Menambahkan baris data
                int row = 2;
                foreach (var item in data)
                {
                    // Menetapkan nilai sel pada worksheet
                    ws.Cells[$"A{row}"].Value = item.IdBooking ?? "N/A";
                    ws.Cells[$"B{row}"].Value = item.IdVehicleNavigation?.IdCustomerNavigation?.Name ?? "N/A";
                    ws.Cells[$"C{row}"].Value = item.OrderDate?.ToString("yyyy-MM-dd") ?? "N/A";
                    ws.Cells[$"D{row}"].Value = item.IdVehicleNavigation?.Type ?? "N/A";
                    ws.Cells[$"E{row}"].Value = item.IdVehicleNavigation?.PoliceNumber ?? "N/A";
                    ws.Cells[$"F{row}"].Value = item.Odometer.ToString() ?? "N/A";
                    ws.Cells[$"G{row}"].Value = item.Complaint ?? "N/A";
                    ws.Cells[$"H{row}"].Value = item.StartRepairTime?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
                    ws.Cells[$"I{row}"].Value = item.EndRepairTime?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
                    ws.Cells[$"J{row}"].Value = item.FinishEstimationTime?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
                    ws.Cells[$"K{row}"].Value = item.RepairDescription ?? "N/A";
                    ws.Cells[$"L{row}"].Value = item.ReplacementPart ?? "N/A";
                    ws.Cells[$"M{row}"].Value = item.Price.ToString() ?? "N/A";
                    ws.Cells[$"N{row}"].Value = item.Evaluation ?? "N/A";
                    ws.Cells[$"O{row}"].Value = item.RepairMethod ?? "N/A";
                    ws.Cells[$"P{row}"].Value = item.AdditionalReplacementPart ?? "N/A";
                    ws.Cells[$"Q{row}"].Value = item.AdditionalPrice.ToString() ?? "N/A";
                    ws.Cells[$"R{row}"].Value = (item.Decision == 1) ? "Ya" : "Tidak";

                    row++;
                }

                // Mengembalikan data Excel sebagai byte array
                return pack.GetAsByteArray();
            }
        }

        public IActionResult ExportPdf()
        {
            // Create a MemoryStream to store the PDF
            using (MemoryStream stream = new MemoryStream())
            {
                // Create a PdfWriter object to write the PDF stream
                using (PdfWriter writer = new PdfWriter(stream))
                {
                    // Create a PdfDocument object
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        // Create a Document object to add elements
                        Document document = new Document(pdf);

                        // Add content to the PDF
                        document.Add(new Paragraph("Hello World"));

                        // Close the document
                        document.Close();
                    }
                }

                // Return the PDF as a file
                return File(stream.ToArray(), "application/pdf", "Document.pdf");
            }
        }

        // Menampilkan halaman pembuatan pemesanan baru
        [AuthorizedCustomer]
        public IActionResult Create(string RepairMethod)
        {
            // Mengambil informasi pelanggan dari sesi
            string authentication = HttpContext.Session.GetString("authentication");
            MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

            // Memastikan pelanggan terautentikasi sebelum menampilkan halaman pembuatan pemesanan
            if (customer == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Memastikan url diakses melalui halaman menu, tidak salah akibat diketik manual
            string[] validMethod = { "FAST TRACK", "TEFA" };
            if (!validMethod.Contains(RepairMethod))
            {
                return RedirectToAction(nameof(Index));
            }

            // Menyiapkan daftar kendaraan pelanggan untuk dipilih dalam pemesanan
            ViewData["IdVehicle"] = new SelectList(_context.MsVehicles.Where(c => c.IdCustomer == customer.IdCustomer && c.Classify != "NONAKTIF"), "IdVehicle", "Type");

            // Inisiasi objek booking untuk kondisi metode TEFA atau FAST TRACK
            TrsBooking trsBooking = new TrsBooking();
            trsBooking.RepairMethod = RepairMethod;

            if (RepairMethod == "FAST TRACK")
            {
                trsBooking.OrderDate = DateTime.Today;
            }

            // Menampilkan halaman pembuatan pemesanan
            return View(trsBooking);
        }

        // Proses pembuatan pemesanan baru oleh pelanggan
        [AuthorizedCustomer]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBooking,OrderDate,IdVehicle,Odometer,Complaint,IdCustomer,RepairMethod")] TrsBooking trsBooking)
        {
            // Memeriksa apakah OrderDate tidak berada diatas H+1
            if (trsBooking.OrderDate <= DateTime.Now.AddDays(1))
            {
                // Mengambil informasi pelanggan dari sesi
                string authentication = HttpContext.Session.GetString("authentication");
                MsCustomer customer = JsonConvert.DeserializeObject<MsCustomer>(authentication);

                TempData["ErrorMessage"] = "Tanggal yang valid minimum H+1 dari hari ini";
                ViewData["IdVehicle"] = new SelectList(_context.MsVehicles.Where(c => c.IdCustomer == customer.IdCustomer), "IdVehicle", "Type");
                return View(trsBooking);
            }

            // Memeriksa jumlah pemesanan maksimal 10 per hari

            var countBookingToday = await _context.TrsBookings.CountAsync(t => t.OrderDate.HasValue && t.OrderDate.Value.Date == trsBooking.OrderDate);
            if (countBookingToday >= 10)
            {
                TempData["ErrorMessage"] = "Maaf kapasitas booking hari ini sudah penuh (10 per hari)";
                return View(trsBooking);
            }

            // Menghasilkan ID pemesanan
            string IdBooking = $"BKN{_context.TrsBookings.Count() + 1}";

            // Menetapkan ID pemesanan
            trsBooking.IdBooking = IdBooking;

            // Status perbaikan default
            trsBooking.RepairStatus = "MENUNGGU";

            // Menambahkan pemesanan ke database
            _context.Add(trsBooking);
            await _context.SaveChangesAsync();

            // Menampilkan pesan sukses ke view
            TempData["SuccessMessage"] = "Booking berhasil!";

            // Mengarahkan ke halaman daftar pemesanan
            return RedirectToAction("Index", "Booking");
        }

        // Memeriksa keberadaan pemesanan berdasarkan ID
        private bool TrsBookingExists(string id)
        {
            // Menggunakan "?." untuk mengatasi null reference jika _context.TrsBookings adalah null
            return (_context.TrsBookings?.Any(e => e.IdBooking == id)).GetValueOrDefault();
        }
    }
}
