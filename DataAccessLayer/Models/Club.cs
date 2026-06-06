using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Club
{
    public long Clubid { get; set; }

    public string Clubcode { get; set; } = null!;

    public string Clubname { get; set; } = null!;

    public string? Description { get; set; }

    public string? Logoimage { get; set; }

    public string? Fanpageurl { get; set; }

    public int Totalactivemembers { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? Foundeddate { get; set; }

    public DateTime Createdat { get; set; }

    public virtual ICollection<Clubboard> Clubboards { get; set; } = new List<Clubboard>();

    public virtual ICollection<Clubreport> Clubreports { get; set; } = new List<Clubreport>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
