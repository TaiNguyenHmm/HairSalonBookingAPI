using AutoMapper;
using DataAccessObjects.Models; 
namespace BusinessObjects
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}