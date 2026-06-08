using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Participant
{
    public long Participantid { get; set; }

    public long Eventid { get; set; }

    public long Userid { get; set; }

    public string? Feedback { get; set; }

    public string Roleinevent { get; set; } = null!;

    public decimal? Evaluationscore { get; set; }

    public string Attendancestatus { get; set; } = null!;

    public DateTime? Checkedinat { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();

    public virtual User User { get; set; } = null!;
}
