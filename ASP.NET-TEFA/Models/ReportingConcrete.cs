using ASP.NET_TEFA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml.Table;
using OfficeOpenXml;
using System.Globalization;

namespace ASP.NET_TEFA.Models
{
    public class ReportingConcrete : IReporting
    {
        private readonly ApplicationDbContext _context;
        public ReportingConcrete(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TrsBooking> GetReportTrsBooking(string monthString)
        {
            try
            {
                IQueryable<TrsBooking> query = _context.TrsBookings
                .Include(t => t.IdVehicleNavigation)
                .ThenInclude(v => v.IdCustomerNavigation)
                .Where(t => t.RepairStatus == "SELESAI");

                if (DateTime.TryParseExact(monthString, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth))
                {
                    int month = parsedMonth.Month;

                    query = query.Where(t => t.OrderDate != null && t.OrderDate.Value.Month == month);
                }

                var listofbookings = query
                    .OrderBy(t => t.OrderDate)
                    .ToList();

                return listofbookings;
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
