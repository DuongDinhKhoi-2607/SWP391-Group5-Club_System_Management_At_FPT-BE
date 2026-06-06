using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Semester
{
    public long Semesterid { get; set; }

    public string Semestername { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public virtual ICollection<Reportperiod> Reportperiods { get; set; } = new List<Reportperiod>();
}
