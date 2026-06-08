using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Department
{
    public long Departmentid { get; set; }

    public string Departmentname { get; set; } = null!;

    public string? Officelocation { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Createdat { get; set; }
}
