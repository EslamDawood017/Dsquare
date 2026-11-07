using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyHub.API.Controllers;

[ApiController]
[Route("wallets")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("{walletId}")]
    public IActionResult GetWallet(string walletId)
    {
        var result = _walletService.GetWallet(walletId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return MapErrorToResponse(result.Error!);
    }

    private IActionResult MapErrorToResponse(ApplicationError error)
    {
        var body = new
        {
            error.Code,
            error.Message,
            error.Details
        };

        switch (error.Type)
        {
            case ErrorType.Validation: return BadRequest(body);
            case ErrorType.NotFound:   return NotFound(body);
            default:                   return StatusCode(500, body);
        }
    }
}
