using AutoMapper;
using DataAccessObjects.Models;

namespace BusinessObjects
{
    public class StylistWorkingHoursProfile : Profile
    {
        public StylistWorkingHoursProfile()
        {
            CreateMap<StylistWorkingHour, StylistWorkingHoursDto>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")));
        }
    }
}
