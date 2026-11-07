using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.DTOs;

namespace LoyaltyHub.Application.Interfaces.Services;

public interface IAwardService
{
    Result<AwardResponseDto> ProcessAward(AwardRequestDto? request);
}

