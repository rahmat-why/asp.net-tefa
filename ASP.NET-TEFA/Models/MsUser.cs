using System;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models;

public partial class MsUser
{
    public string IdUser { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Nim { get; set; }

    public string? Nidn { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Position { get; set; }

    public virtual ICollection<TrsBooking> TrsBookingHeadMechanicNavigations { get; set; } = new List<TrsBooking>();

    public virtual ICollection<TrsBooking> TrsBookingServiceAdvisorNavigations { get; set; } = new List<TrsBooking>();
}
