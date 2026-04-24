using AutoMapper;
using CLS.BLL.DTOs.Sessions;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class SessionMappingProfile : Profile
{
    public SessionMappingProfile()
    {
        // Session → SessionResponse (flatten navigation properties)
        CreateMap<Session, SessionResponse>()
            .ForMember(d => d.ClassName,    o => o.MapFrom(s => s.Class.Name))
            .ForMember(d => d.TeacherName,  o => o.MapFrom(s => s.Teacher.FullName))
            .ForMember(d => d.RoomName,     o => o.MapFrom(s => s.Room.Name));

        // Class → ClassDto
        CreateMap<Class, ClassDto>();

        // Room → RoomDto
        CreateMap<Room, RoomDto>();

        // User → TeacherDto
        CreateMap<User, TeacherDto>();
    }
}
