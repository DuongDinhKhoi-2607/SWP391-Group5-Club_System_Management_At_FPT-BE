using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Userinformation
{
    public long Userinfoid { get; set; }

    public long Userid { get; set; }

    public string? Studentid { get; set; }

    public string Fullname { get; set; } = null!;

    public string Schoolemail { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public string? Major { get; set; }

    public string? Academicbatch { get; set; }

    public string? Avatar { get; set; }

    public string? Gender { get; set; }

    public bool Isalumni { get; set; }

    public DateOnly? Dateofbirth { get; set; }

    public DateOnly? Graduationdate { get; set; }

    public DateTime Infoupdatedat { get; set; }

    public virtual User User { get; set; } = null!;
}
