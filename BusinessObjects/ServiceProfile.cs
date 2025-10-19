using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
  public class ServiceProfile : AutoMapper.Profile
  {
    public ServiceProfile()
    {
      CreateMap<DataAccessObjects.Models.Service, ServiceDto>().ReverseMap();
    }
    }
}
