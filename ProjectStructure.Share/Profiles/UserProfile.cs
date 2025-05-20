using AutoMapper;
using ProjectStructure.Share.DTOs.User;
using ProjectStructure.Share.Entities.MsSQL;
using ProjectStructure.Share.Extensions;

namespace ProjectStructure.Share.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity 到 DTO 的映射
            CreateMap<User, OutUserDto>();

            // DTO 到 Entity 的映射
            CreateMap<InCreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password.ToSHA256()))
                .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<InUpdateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
