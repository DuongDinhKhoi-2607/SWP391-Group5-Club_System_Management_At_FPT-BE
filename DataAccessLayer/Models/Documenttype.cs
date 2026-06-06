using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Documenttype
{
    public long Documenttypeid { get; set; }

    public string Typename { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Createdat { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
