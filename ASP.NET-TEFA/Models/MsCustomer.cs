using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class MsCustomer
{
    public string IdCustomer { get; set; } = null!;

    [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email must match with format")]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = null!;

    [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "Customer name can only be letters and spaces")]
    [Required(ErrorMessage = "Customer name is required")]
    public string Name { get; set; } = null!;

    [RegularExpression("^[0-9]+$", ErrorMessage = "Phone number can only be a number")]
    [Required(ErrorMessage = "Phone number is required")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; } = null!;

    public string? Password { get; set; }

    public virtual ICollection<MsVehicle> MsVehicles { get; set; } = new List<MsVehicle>();
}