using ASP.NET_TEFA.Models;
using System.ComponentModel.DataAnnotations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class MsUser
{
    public string IdUser { get; set; } = null!;

    [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "Full name can only be letters and spaces")]
    [Required(ErrorMessage = "Full name is required")]
    public string? FullName { get; set; }

    [RegularExpression("^[0-9]{10}$", ErrorMessage = "NIM must consist of 10 digits")]
    [Required(ErrorMessage = "NIM is required")]
    public string? Nim { get; set; }

    public string? Nidn { get; set; }

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain letters and numbers")]
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Passwords must contain letters and numbers")]
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Position is required")]
    public string? Position { get; set; }

    public virtual ICollection<TrsBooking> TrsBookingHeadMechanicNavigations { get; set; } = new List<TrsBooking>();

    public virtual ICollection<TrsBooking> TrsBookingServiceAdvisorNavigations { get; set; } = new List<TrsBooking>();

    public virtual ICollection<TrsPending> TrsPendings { get; set; } = new List<TrsPending>();
}