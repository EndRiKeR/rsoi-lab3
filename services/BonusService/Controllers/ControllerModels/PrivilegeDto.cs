using BonusService.Database.Models;

namespace BonusService.Controllers.ControllerModels;

public class PrivilegeDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Status { get; set; } = "BRONZE";
    public int? Balance { get; set; }

    public PrivilegeDto(Privilege privilege)
    {
        Id = privilege.Id;
        Username = privilege.Username;
        Status = privilege.Status;
        Balance = privilege.Balance;
    }
}