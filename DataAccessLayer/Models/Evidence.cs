using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Evidence
{
    public long Evidenceid { get; set; }

    public long Participantid { get; set; }

    public string Evidencename { get; set; } = null!;

    public string Fileurl { get; set; } = null!;

    public string Isverified { get; set; } = null!;

    public DateTime Uploadedat { get; set; }

    public DateTime? Verifiedat { get; set; }

    public virtual Participant Participant { get; set; } = null!;
}
