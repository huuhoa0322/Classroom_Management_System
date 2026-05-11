using AutoMapper;
using CLS.BLL.DTOs.ActivityLogs;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

/// <summary>AutoMapper profile: ActivityLog → ActivityLogResponse.</summary>
public class ActivityLogMappingProfile : Profile
{
    public ActivityLogMappingProfile()
    {
        CreateMap<ActivityLog, ActivityLogResponse>()
            .ForMember(dest => dest.UserFullName,
                       opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Unknown"));
    }
}
