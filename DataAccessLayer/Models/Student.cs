using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Student
{
    public string Studentid { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string Schoolemail { get; set; } = null!;

    public string? Major { get; set; }

    public string? Academicbatch { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Dateofbirth { get; set; }

    public string Status { get; set; } = null!;

    public virtual Userinformation? Userinformation { get; set; }
}
