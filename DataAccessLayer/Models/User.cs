using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class User
{
    public long Userid { get; set; }

    public long? Departmentid { get; set; }

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Systemrole { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public DateTime? Lastloginat { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public virtual Userinformation? Userinformation { get; set; }
}
