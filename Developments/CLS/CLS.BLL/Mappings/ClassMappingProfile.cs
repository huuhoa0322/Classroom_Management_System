using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Classes;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

/// <summary>AutoMapper profile cho Class feature.</summary>
public class ClassMappingProfile : Profile
{
    public ClassMappingProfile()
    {
        CreateMap<Class, ClassResponse>()
            .ForMember(d => d.StudentCount,
                       o => o.MapFrom(s => s.ClassStudents.Count(
                           cs => cs.Status == AppConstants.ClassStudentStatus.Active)))
            .ForMember(d => d.SessionCount,
                       o => o.MapFrom(s => s.Sessions.Count));

        CreateMap<ClassStudent, ClassStudentResponse>()
            .ForMember(d => d.StudentName,
                       o => o.MapFrom(s => s.Student.FullName));
    }
}
