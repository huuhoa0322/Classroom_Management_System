using AutoMapper;
using CLS.BLL.DTOs.Feedback;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class FeedbackMappingProfile : Profile
{
    public FeedbackMappingProfile()
    {
        CreateMap<Feedback, FeedbackDto>()
            .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student.FullName));
    }
}
