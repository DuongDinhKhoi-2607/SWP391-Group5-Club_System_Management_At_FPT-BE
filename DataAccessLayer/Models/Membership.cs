using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Membership
{
    public long Membershipid { get; set; }

    public long Userid { get; set; }

    public long Clubid { get; set; }

    public string? Personalgoal { get; set; }

    public string? Joinreason { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly Joindate { get; set; }

    public DateOnly? Leftdate { get; set; }

    public virtual ICollection<Boardmember> Boardmembers { get; set; } = new List<Boardmember>();

    public virtual Club Club { get; set; } = null!;
}
