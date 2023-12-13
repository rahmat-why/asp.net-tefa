using ASP.NET_TEFA.Models;
using System.ComponentModel.DataAnnotations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class MsUser
{
    public string IdUser { get; set; } = null!;

    [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "Nama lengkap hanya boleh huruf dan spasi.")]
    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    public string? FullName { get; set; }

    [RegularExpression("^[0-9]{10}$", ErrorMessage = "NIM harus terdiri dari 10 angka.")]
    [Required(ErrorMessage = "NIM wajib diisi.")]
    public string? Nim { get; set; }

    public string? Nidn { get; set; }

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Username harus mengandung huruf dan angka")]
    [Required(ErrorMessage = "Username wajib diisi")]
    public string? Username { get; set; }

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Password harus mengandung huruf dan angka")]
    [Required(ErrorMessage = "Password wajib diisi")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Posisi wajib diisi.")]
    public string? Position { get; set; }

    public virtual ICollection<TrsBooking> TrsBookingHeadMechanicNavigations { get; set; } = new List<TrsBooking>();

    public virtual ICollection<TrsBooking> TrsBookingServiceAdvisorNavigations { get; set; } = new List<TrsBooking>();

    public virtual ICollection<TrsPending> TrsPendings { get; set; } = new List<TrsPending>();
}