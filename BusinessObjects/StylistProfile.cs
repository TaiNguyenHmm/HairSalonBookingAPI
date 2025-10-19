using AutoMapper;
using DataAccessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StylistProfile : Profile
    {
        public StylistProfile()
        {
            CreateMap<Stylist, StylistDto>().ReverseMap();
        }
    }
    }

