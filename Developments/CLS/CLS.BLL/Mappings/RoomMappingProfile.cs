using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Rooms;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class RoomMappingProfile : Profile
{
    public RoomMappingProfile()
    {
        CreateMap<Room, RoomResponse>();
    }
}
