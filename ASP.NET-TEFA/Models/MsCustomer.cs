using System;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models;

public partial class MsCustomer
{
    public string IdCustomer { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Password { get; set; }

    public virtual ICollection<MsVehicle> MsVehicles { get; set; } = new List<MsVehicle>();
}
