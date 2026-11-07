using AutoMapper;
using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.DTOs;
using LoyaltyHub.Application.Errors;
using LoyaltyHub.Application.Interfaces.Repositories;
using LoyaltyHub.Application.Interfaces.Services;
using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Application.Services;

public class AwardService : IAwardService
{
    private static readonly object _lock = new();

    private readonly IAwardRepository _awardRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public AwardService(
        IAwardRepository awardRepository,
        IWalletRepository walletRepository,
        IMapper mapper)
    {
        _awardRepository = awardRepository;
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public Result<AwardResponseDto> ProcessAward(AwardRequestDto? request)
    {
        if (request == null)
            return Result<AwardResponseDto>.Failure(ApplicationErrors.InvalidRequest("Request body cannot be null."));

        lock (_lock)
        {
            var existingAward = _awardRepository.GetByIdentity(request.SourceId, request.ExternalTransactionId);
            if (existingAward != null)
            {
                if (existingAward.WalletId == request.WalletId &&
                    existingAward.Points == request.Points)
                {
                    var existingResponse = _mapper.Map<AwardResponseDto>(existingAward);
                    existingResponse.IsNewAward = false;
                    return Result<AwardResponseDto>.Success(existingResponse);
                }

                return Result<AwardResponseDto>.Failure(ApplicationErrors.ConflictingRetry);
            }

            var wallet = _walletRepository.GetById(request.WalletId);
            if (wallet == null)
            {
                return Result<AwardResponseDto>.Failure(ApplicationErrors.WalletNotFound(request.WalletId));
            }

            wallet.Balance += request.Points;
            _walletRepository.Update(wallet);

            var newAward = _mapper.Map<Award>(request);
            _awardRepository.Add(newAward);

            return Result<AwardResponseDto>.Success(_mapper.Map<AwardResponseDto>(newAward));
        }
    }
}
