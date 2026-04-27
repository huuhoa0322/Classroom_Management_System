using AutoMapper;
using CLS.BLL.DTOs.Attendance;
using CLS.BLL.DTOs.Sessions;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class AttendanceMappingProfile : Profile
{
    public AttendanceMappingProfile()
    {
        // Attendance → AttendanceDto (flatten Student navigation)
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(a => a.Student.FullName));

        // Session → SessionResponse (reuse existing mapping in SessionMappingProfile)
        // No additional mapping needed here — SessionMappingProfile already handles it.
    }
}
