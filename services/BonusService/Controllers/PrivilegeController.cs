using BonusService.Controllers.ControllerModels;
using BonusService.Database.Models;
using BonusService.Database.Repositories.Interfaces;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Microsoft.AspNetCore.Mvc;

namespace BonusService.Controllers
{
    [ApiController]
    [Route("api/v1/privilege")]
    public class PrivilegeController : ControllerBase
    {
        private readonly IPrivilegeRepository _privilegeRepository;
        private readonly IPrivilegeHistoryRepository _privilegeHistoryRepository;
        
        public PrivilegeController(
            IPrivilegeRepository privilegeRepository,
            IPrivilegeHistoryRepository privilegeHistoryRepository)
        {
            _privilegeRepository = privilegeRepository;
            _privilegeHistoryRepository = privilegeHistoryRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPrivilegeInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                Privilege privilege;
                try
                {
                    privilege = await _privilegeRepository.GetByUsername(username.ToString());
                }
                catch (Exception)
                {
                    privilege = new Privilege
                    {
                        Username = username.ToString(),
                        Status = "BRONZE",
                        Balance = 0
                    };
                    privilege = await _privilegeRepository.Add(privilege);
                }
                
                var history = await _privilegeHistoryRepository.GetByPrivilegeId(privilege.Id);
                
                var historyResponses = history.Select(h => new BalanceHistory
                {
                    Date = h.Datetime,
                    TicketUid = h.TicketUid,
                    BalanceDiff = h.BalanceDiff,
                    OperationType = h.OperationType
                }).ToList();
                
                var response = new PrivilegeInfoResponse
                {
                    Balance = privilege.Balance ?? 0,
                    Status = privilege.Status,
                    History = historyResponses
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpPost("update-balance")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceHistoryRequest historyRequest)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValues))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var username = usernameValues[0];
                PrivilegeDto privilege;

                if (await _privilegeRepository.ExistsByUsername(username))
                {
                    Console.WriteLine($"exists");
                    privilege = new PrivilegeDto(await _privilegeRepository.GetByUsername(username));
                    await _privilegeRepository.UpdateBalance(privilege.Id, historyRequest.BalanceDiff);
                }
                else
                {
                    Console.WriteLine($"not exists");
                    privilege = new PrivilegeDto(await _privilegeRepository.Add(new Privilege
                    {
                        Id = -1,
                        Username = username,
                        Status = "BRONZE",
                        Balance = 0,
                    }));
                }
                
                var history = new PrivilegeHistory
                {
                    PrivilegeId = privilege.Id,
                    TicketUid = historyRequest.TicketUid,
                    Datetime = DateTime.UtcNow,
                    BalanceDiff = historyRequest.BalanceDiff,
                    OperationType = historyRequest.OperationType
                };
                
                await _privilegeHistoryRepository.Add(history);
                
                return Ok(privilege);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        // bonuses - "DEBIT_THE_ACCOUNT" : money - "FILL_IN_BALANCE"
        [HttpPost("return-balance")]
        public async Task<IActionResult> ReturnBalance([FromBody] ReturnBalanceHistoryRequest request)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValues))
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });

                var history = (await _privilegeHistoryRepository.GetByTicketUid(request.TicketUid))[^1];
                var username = usernameValues[0];
                PrivilegeDto privilege = new PrivilegeDto(await _privilegeRepository.GetByUsername(username));
                
                await _privilegeRepository.UpdateBalance(privilege.Id, -history.BalanceDiff);
                
                return Ok(privilege);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
    }
}