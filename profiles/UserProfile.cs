using AutoMapper;
using Backend.DTOs.auth;
using Backend.models;
using System;

namespace Backend.mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            //-= RegisterDto -> User
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.userID, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.password, opt => opt.Ignore()) // Password should be hashed before saving)
                .ForMember(dest => dest.organisationID, opt => opt.MapFrom(src => src.organisationID))
                .ForMember(dest => dest.role, opt => opt.MapFrom(src => src.role))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.lastLogin, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.refreshToken, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.tokenExpiry, opt => opt.MapFrom(src => DateTime.Now.AddDays(7)));

            //-= UpdateUserDto -> User
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.password, opt => opt.Ignore()) // Password should be hashed before saving)
                .ForMember(dest => dest.organisationID, opt => opt.MapFrom(src => src.organisationID));
;
        }
        
    }

}
