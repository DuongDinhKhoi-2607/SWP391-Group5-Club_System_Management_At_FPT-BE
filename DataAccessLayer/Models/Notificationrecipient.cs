namespace DataAccessLayer.Models;

public partial class Notificationrecipient
{
    public long Notificationrecipientid { get; set; }
    public long Notificationid { get; set; }
    public long Userid { get; set; }
    public bool Isread { get; set; }
    public DateTime? Readat { get; set; }

    public virtual Notification Notification { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
