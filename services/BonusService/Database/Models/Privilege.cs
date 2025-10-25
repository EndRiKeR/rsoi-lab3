namespace BonusService.Database.Models;

public class Privilege
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Status { get; set; } = "BRONZE";
    public int? Balance { get; set; }
    
    public virtual ICollection<PrivilegeHistory> History { get; set; } = new List<PrivilegeHistory>();
}