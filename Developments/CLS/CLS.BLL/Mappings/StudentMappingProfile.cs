using AutoMapper;
using CLS.BLL.DTOs.Students;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class StudentMappingProfile : Profile
{
    public StudentMappingProfile()
    {
        // Student Entity → StudentResponse (flatten Parent fields)
        CreateMap<Student, StudentResponse>()
            .ForMember(d => d.ParentId,           o => o.MapFrom(s => s.Parent.Id))
            .ForMember(d => d.ParentFullName,      o => o.MapFrom(s => s.Parent.FullName))
            .ForMember(d => d.ParentEmail,         o => o.MapFrom(s => s.Parent.Email))
            .ForMember(d => d.ParentPhone,         o => o.MapFrom(s => s.Parent.Phone))
            .ForMember(d => d.ParentRelationship,  o => o.MapFrom(s => s.Parent.Relationship));

        // CreateStudentRequest → Student (chỉ fields của Student)
        // EnrolledAt được set trong Service (nguồn duy nhất của truth — không set ở Mapper)
        CreateMap<CreateStudentRequest, Student>()
            .ForMember(d => d.EnrolledAt, o => o.Ignore())    // set trong Service.CreateAsync
            .ForMember(d => d.Status,     o => o.MapFrom(_ => "active"))
            .ForMember(d => d.ParentId,   o => o.Ignore())    // set qua FK hoặc navigation property
            .ForMember(d => d.Parent,     o => o.Ignore());   // service gán thủ công sau upsert

        // CreateStudentRequest → Parent (chỉ fields của Parent)
        CreateMap<CreateStudentRequest, Parent>()
            .ForMember(d => d.FullName,      o => o.MapFrom(s => s.ParentFullName))
            .ForMember(d => d.Email,         o => o.MapFrom(s => s.ParentEmail))
            .ForMember(d => d.Phone,         o => o.MapFrom(s => s.ParentPhone))
            .ForMember(d => d.Relationship,  o => o.MapFrom(s => s.ParentRelationship));

        // UpdateStudentRequest → Student
        CreateMap<UpdateStudentRequest, Student>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));
    }
}
