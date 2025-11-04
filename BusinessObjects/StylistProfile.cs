using AutoMapper;
using DataAccessObjects.Models;


namespace BusinessObjects
{
    public class StylistProfile : Profile
    {
        public StylistProfile()
        {
            CreateMap<Stylist, StylistDto>().ReverseMap();
            CreateMap<Stylist, StylistDto>()
     .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
     .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.Phone))
     .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));

        }
    }
    }

