using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class MsVehicle
{
    [Required(ErrorMessage = "Id Vehicle is required")]
    public string? IdVehicle { get; set; }

    [RegularExpression("^[A-Za-z0-9 ]+$", ErrorMessage = "Vehicle type must contain letters, numbers, and spaces only")]
    [Required(ErrorMessage = "Vehicle type is required")]
    public string Type { get; set; } = null!;

    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Classify must contain letters and numbers only")]
    [Required(ErrorMessage = "Classify is required")]
    public string Classify { get; set; } = null!;

    [RegularExpression("^[A-Za-z0-9 ]+$", ErrorMessage = "Police number must contain letters and numbers only")]
    [Required(ErrorMessage = "Police number is required")]
    public string PoliceNumber { get; set; } = null!;

    [RegularExpression("^[A-Za-z ]+$", ErrorMessage = "Color must contain letters and spaces only")]
    [Required(ErrorMessage = "Color is required")]
    public string Color { get; set; } = null!;

    [RegularExpression("^[0-9]+$", ErrorMessage = "Year must contain numbers only")]
    [Required(ErrorMessage = "Year is required")]
    public int Year { get; set; }

    [RegularExpression("^[A-Za-z ]+$", ErrorMessage = "Vehicle owner must contain letters and spaces only")]
    [Required(ErrorMessage = "Vehicle owner is required")]
    public string VehicleOwner { get; set; } = null!;

    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Chassis numbers must contain letters and numbers only")]
    public string? ChassisNumber { get; set; }

    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Machine numbers must contain letters and numbers only")]
    public string? MachineNumber { get; set; }

    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Id Customer must contain letters and numbers only")]
    [Required(ErrorMessage = "Id Customer is required")]
    public string IdCustomer { get; set; } = null!;

    public virtual MsCustomer IdCustomerNavigation { get; set; } = null!;

    public virtual ICollection<TrsBooking> TrsBookings { get; set; } = new List<TrsBooking>();
}
