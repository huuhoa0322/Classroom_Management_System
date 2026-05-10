using AutoMapper;
using CLS.BLL.DTOs.Users;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponse>();
    }
}
