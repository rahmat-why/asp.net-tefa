using System;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models;

public partial class TrsBooking
{
    public string IdBooking { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public string? IdVehicle { get; set; }

    public int? Odometer { get; set; }

    public string? Complaint { get; set; }

    public virtual MsVehicle? IdVehicleNavigation { get; set; }
}
