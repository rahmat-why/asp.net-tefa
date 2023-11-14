using ASP.NET_TEFA.Models;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models
{
    public interface IReporting
    {
        List<TrsBooking> GetReportTrsBooking(string monthString);
    }
}