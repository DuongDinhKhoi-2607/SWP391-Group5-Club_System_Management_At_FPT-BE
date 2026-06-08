using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Event
{
    public long Eventid { get; set; }

    public long Clubid { get; set; }

    public string Eventname { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? Planbudget { get; set; }

    public int Targetparticipants { get; set; }

    public int Actualparticipants { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Starttime { get; set; }

    public DateTime Endtime { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
