namespace Common.DtoModels.BonusServiceDto;

public class UpdateBalanceHistoryRequest
{
    public Guid TicketUid { get; set; }
    public int BalanceDiff { get; set; }
    public string OperationType { get; set; } = string.Empty;
}