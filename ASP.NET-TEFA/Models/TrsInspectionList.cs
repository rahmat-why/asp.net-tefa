using System;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models;

public partial class TrsInspectionList
{
    public string IdInspection { get; set; } = null!;

    public string? IdBooking { get; set; }

    public string? IdEquipment { get; set; }

    public int? Checklist { get; set; }

    public string? Description { get; set; }

    public virtual TrsBooking? IdBookingNavigation { get; set; }

    public virtual MsEquipment? IdEquipmentNavigation { get; set; }
}
