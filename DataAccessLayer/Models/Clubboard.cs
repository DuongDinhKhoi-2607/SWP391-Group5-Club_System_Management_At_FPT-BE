using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Clubboard
{
    public long Boardid { get; set; }

    public long Clubid { get; set; }

    public string Boardname { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public virtual ICollection<Boardmember> Boardmembers { get; set; } = new List<Boardmember>();

    public virtual Club Club { get; set; } = null!;
}
