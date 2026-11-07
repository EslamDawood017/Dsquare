using AutoMapper;
using LoyaltyHub.Application.DTOs;
using LoyaltyHub.Domain.Entities;

namespace LoyaltyHub.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Award, AwardResponseDto>()
            .ForMember(dest => dest.IsNewAward, opt => opt.MapFrom(_ => true));

        CreateMap<Wallet, WalletResponseDto>();

        CreateMap<AwardRequestDto, Award>()
            .ForMember(dest => dest.AwardId, opt => opt.MapFrom(_ => Guid.NewGuid().ToString("N")))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}
