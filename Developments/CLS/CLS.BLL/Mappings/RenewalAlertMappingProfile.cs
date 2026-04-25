using AutoMapper;
using CLS.BLL.DTOs.RenewalAlerts;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class RenewalAlertMappingProfile : Profile
{
    public RenewalAlertMappingProfile()
    {
        CreateMap<AlertNotification, RenewalAlertResponse>()
            .ForMember(d => d.StudentName,
                o => o.MapFrom(s => s.StudentPackage != null ? s.StudentPackage.Student.FullName : "—"))
            .ForMember(d => d.ParentName,
                o => o.MapFrom(s => s.StudentPackage != null ? s.StudentPackage.Student.Parent.FullName : "—"))
            .ForMember(d => d.ParentEmail,
                o => o.MapFrom(s => s.StudentPackage != null ? s.StudentPackage.Student.Parent.Email : s.TargetEmail))
            .ForMember(d => d.ParentPhone,
                o => o.MapFrom(s => s.StudentPackage != null ? s.StudentPackage.Student.Parent.Phone : null))
            .ForMember(d => d.PackageName,
                o => o.MapFrom(s => s.StudentPackage != null ? s.StudentPackage.Package.Name : "—"))
            .ForMember(d => d.RemainingSessions,
                o => o.MapFrom(s => s.StudentPackage != null ? s.StudentPackage.RemainingSessions : 0))
            .ForMember(d => d.RemainingDays,
                o => o.MapFrom(s => s.StudentPackage != null
                    ? (s.StudentPackage.EndDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                    : 0));
    }
}
