using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Document
{
    public long Documentid { get; set; }

    public long Clubid { get; set; }

    public long Documenttypeid { get; set; }

    public string Documentname { get; set; } = null!;

    public string Fileurl { get; set; } = null!;

    public long Filesize { get; set; }

    public int Downloadcount { get; set; }

    public string Accesslevel { get; set; } = null!;

    public DateTime Uploadedat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual Documenttype Documenttype { get; set; } = null!;
}
