using System;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models;

public partial class MsVehicle
{
    public string IdVehicle { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Classify { get; set; } = null!;

    public string PoliceNumber { get; set; } = null!;

    public string Color { get; set; } = null!;

    public int Yeart { get; set; }

    public string VehicleOwner { get; set; } = null!;

    public string ChassisNumber { get; set; } = null!;

    public string MachineNumber { get; set; } = null!;

    public virtual ICollection<TrsBooking> TrsBookings { get; set; } = new List<TrsBooking>();
}
