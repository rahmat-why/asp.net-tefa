using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class MsVehicle
{
    [Required(ErrorMessage = "Id Kendaraan wajib diisi")]
    public string? IdVehicle { get; set; }

    [RegularExpression("^[A-Za-z0-9 ]+$", ErrorMessage = "Tipe harus mengandung huruf, angka, dan spasi saja")]
    [Required(ErrorMessage = "Tipe wajib diisi")]
    public string Type { get; set; } = null!;

    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Klasifikasi harus mengandung huruf dan angka saja")]
    [Required(ErrorMessage = "Klasifikasi wajib diisi")]
    public string Classify { get; set; } = null!;

    [RegularExpression("^[A-Za-z0-9 ]+$", ErrorMessage = "Nomor Polisi harus mengandung huruf dan angka saja")]
    [Required(ErrorMessage = "Nomor Polisi wajib diisi")]
    public string PoliceNumber { get; set; } = null!;

    [RegularExpression("^[A-Za-z ]+$", ErrorMessage = "Warna harus mengandung huruf dan spasi saja")]
    [Required(ErrorMessage = "Warna wajib diisi")]
    public string Color { get; set; } = null!;

    [RegularExpression("^[0-9]+$", ErrorMessage = "Tahun harus mengandung angka saja")]
    [Required(ErrorMessage = "Tahun wajib diisi")]
    public int Year { get; set; }

    [RegularExpression("^[A-Za-z ]+$", ErrorMessage = "Pemilik Kendaraan harus mengandung huruf dan spasi saja")]
    [Required(ErrorMessage = "Pemilik Kendaraan wajib diisi")]
    public string VehicleOwner { get; set; } = null!;

    public string? ChassisNumber { get; set; }

    public string? MachineNumber { get; set; }

    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Id Pelanggan harus mengandung huruf dan angka saja")]
    [Required(ErrorMessage = "Id Pelanggan wajib diisi")]
    public string IdCustomer { get; set; } = null!;

    public virtual MsCustomer IdCustomerNavigation { get; set; } = null!;

    public virtual ICollection<TrsBooking> TrsBookings { get; set; } = new List<TrsBooking>();
}
