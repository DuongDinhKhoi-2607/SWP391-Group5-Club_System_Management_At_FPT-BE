using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Clubreport
{
    public long Clubreportid { get; set; }

    public long Clubid { get; set; }

    public long Reportperiodid { get; set; }

    public string Reporttitle { get; set; } = null!;

    public string? Summarycontent { get; set; }

    public string? IcpdpFeedback { get; set; }

    public int Totaleventsheld { get; set; }

    public decimal Financialbalance { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Submittedat { get; set; }

    public DateTime? Reviewedat { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual Reportperiod Reportperiod { get; set; } = null!;
}
