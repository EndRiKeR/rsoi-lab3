namespace BonusService.Database.Models;

public class PrivilegeHistory
{
    public int Id { get; set; }
    public int PrivilegeId { get; set; }
    public Guid TicketUid { get; set; }
    public DateTime Datetime { get; set; }
    public int BalanceDiff { get; set; }
    public string OperationType { get; set; } = string.Empty;
    
    public virtual Privilege Privilege { get; set; } = null!;
}