using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Reportperiod
{
    public long Reportperiodid { get; set; }

    public long Semesterid { get; set; }

    public string Periodname { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Deadline { get; set; }

    public DateTime Createdat { get; set; }

    public virtual ICollection<Clubreport> Clubreports { get; set; } = new List<Clubreport>();

    public virtual Semester Semester { get; set; } = null!;
}
