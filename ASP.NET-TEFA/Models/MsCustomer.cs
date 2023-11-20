using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class MsCustomer
{
    public string IdCustomer { get; set; } = null!;

    [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email harus sesuai format email")]
    [Required(ErrorMessage = "Email wajib diisi")]
    public string Email { get; set; } = null!;

    [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "Nama Customer hanya boleh huruf dan spasi")]
    [Required(ErrorMessage = "Nama Customer wajib diisi")]
    public string Name { get; set; } = null!;

    [RegularExpression("^[0-9]+$", ErrorMessage = "No Telepon hanya boleh angka")]
    [Required(ErrorMessage = "No Telepon wajib diisi")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Alamat Lengkap wajib diisi")]
    public string Address { get; set; } = null!;

    public string? Password { get; set; }

    public virtual ICollection<MsVehicle> MsVehicles { get; set; } = new List<MsVehicle>();
}