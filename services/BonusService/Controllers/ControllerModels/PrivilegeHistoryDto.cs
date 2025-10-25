using BonusService.Database.Models;

namespace BonusService.Controllers.ControllerModels;

public class PrivilegeHistoryDto
{
    public int Id { get; set; }
    public int PrivilegeId { get; set; }
    public Guid TicketUid { get; set; }
    public DateTime Datetime { get; set; }
    public int BalanceDiff { get; set; }
    public string OperationType { get; set; } = string.Empty;

    public PrivilegeHistoryDto(PrivilegeHistory privilegeHistory)
    {
        Id = privilegeHistory.Id;
        PrivilegeId = privilegeHistory.PrivilegeId;
        TicketUid = privilegeHistory.TicketUid;
        Datetime = privilegeHistory.Datetime;
        BalanceDiff = privilegeHistory.BalanceDiff;
        OperationType = privilegeHistory.OperationType;
    }
}