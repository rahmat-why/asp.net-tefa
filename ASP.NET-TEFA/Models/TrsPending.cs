using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class TrsPending
{
    public string IdPending { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime? FinishTime { get; set; }

    public string Reason { get; set; } = null!;

    public string IdBooking { get; set; } = null!;

    public virtual TrsBooking IdBookingNavigation { get; set; } = null!;
}