namespace DataAccessLayer.Models;

public partial class Notification
{
    public long Notificationid { get; set; }
    public long Senderid { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Notificationtype { get; set; } = null!;
    public string Targettype { get; set; } = null!;
    public string? Targetrole { get; set; }
    public long? Clubid { get; set; }
    public long? Eventid { get; set; }
    public long? Clubreportid { get; set; }
    public long? Reportperiodid { get; set; }
    public DateTime Createdat { get; set; }

    public virtual User Sender { get; set; } = null!;
    public virtual Club? Club { get; set; }
    public virtual Event? Event { get; set; }
    public virtual Clubreport? Clubreport { get; set; }
    public virtual Reportperiod? Reportperiod { get; set; }
    public virtual ICollection<Notificationrecipient> Notificationrecipients { get; set; }
        = new List<Notificationrecipient>();
}
