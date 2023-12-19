using System;
using System.Collections.Generic;

namespace ASP.NET_TEFA.Models;

public partial class MsEquipment
{
    public string IdEquipment { get; set; } = null!;

    public string? Name { get; set; }

    public int? IsActive { get; set; }

    public string? Std { get; set; }

    public int? Ordering { get; set; }

    public virtual ICollection<TrsInspectionList> TrsInspectionLists { get; set; } = new List<TrsInspectionList>();
}
