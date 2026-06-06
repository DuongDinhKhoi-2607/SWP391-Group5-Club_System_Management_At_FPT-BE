using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Boardmember
{
    public long Boardmemberid { get; set; }

    public long Boardid { get; set; }

    public long Membershipid { get; set; }

    public string Position { get; set; } = null!;

    public string? Dutydescription { get; set; }

    public string? Handoverdocumenturl { get; set; }

    public decimal? KpiScore { get; set; }

    public DateTime Appointedat { get; set; }

    public virtual Clubboard Board { get; set; } = null!;

    public virtual Membership Membership { get; set; } = null!;
}
