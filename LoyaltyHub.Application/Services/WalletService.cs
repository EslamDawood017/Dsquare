using AutoMapper;
using LoyaltyHub.Application.Common;
using LoyaltyHub.Application.DTOs;
using LoyaltyHub.Application.Errors;
using LoyaltyHub.Application.Interfaces.Repositories;
using LoyaltyHub.Application.Interfaces.Services;
using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public WalletService(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository ;
        _mapper = mapper;
    }

    public Result<WalletResponseDto> GetWallet(string walletId)
    {
        if (string.IsNullOrWhiteSpace(walletId))
        {
            return Result<WalletResponseDto>.Failure(ApplicationErrors.InvalidRequest("walletId is required."));
        }

        var wallet = _walletRepository.GetById(walletId);

        if (wallet == null)
        {
            return Result<WalletResponseDto>.Failure(ApplicationErrors.WalletNotFound(walletId));
        }

        return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(wallet));
    }
}
