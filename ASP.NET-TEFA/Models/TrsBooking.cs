using ASP.NET_TEFA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_TEFA.Models;

public partial class TrsBooking
{
    public string IdBooking { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public string IdVehicle { get; set; } = null!;

    public int? Odometer { get; set; }

    public string? Complaint { get; set; }

    public string? ServiceAdvisor { get; set; }

    public string? HeadMechanic { get; set; }

    public DateTime? StartRepairTime { get; set; }

    public DateTime? EndRepairTime { get; set; }

    public DateTime? FinishEstimationTime { get; set; }

    public string? RepairDescription { get; set; }

    public string? ReplacementPart { get; set; }

    public string? Evaluation { get; set; }

    public int? Price { get; set; }

    public DateTime? CreatedTime { get; set; }

    public string? RepairStatus { get; set; }

    public string? RepairMethod { get; set; }

    public int? Control { get; set; }

    public int? Progress { get; set; }

    public string? AdditionalReplacementPart { get; set; }

    public int? AdditionalPrice { get; set; }

    public int? Decision { get; set; }

    public int? WorkingCost { get; set; }

    public virtual MsUser? HeadMechanicNavigation { get; set; }

    public virtual MsVehicle IdVehicleNavigation { get; set; } = null!;

    public virtual MsUser? ServiceAdvisorNavigation { get; set; }

    public virtual ICollection<TrsInspectionList> TrsInspectionLists { get; set; } = new List<TrsInspectionList>();

    public virtual ICollection<TrsPending> TrsPendings { get; set; } = new List<TrsPending>();
}
