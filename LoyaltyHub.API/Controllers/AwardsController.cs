using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.DTOs;
using LoyaltyHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyHub.API.Controllers;

[ApiController]
[Route("awards")]
public class AwardsController : ControllerBase
{
    private readonly IAwardService _awardService;

    public AwardsController(IAwardService awardService)
    {
        _awardService = awardService;
    }

    [HttpPost]
    public IActionResult ProcessAward([FromBody] AwardRequestDto request)
    {
        var result = _awardService.ProcessAward(request);

        if (result.IsSuccess)
        {
            var award = result.Value;

            if (award.IsNewAward)
                return CreatedAtAction(nameof(ProcessAward), new { id = award.AwardId }, award);

            return Ok(award);
        }

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
            case ErrorType.Conflict:   return Conflict(body);
            default:                   return StatusCode(500, body);
        }
    }
}

