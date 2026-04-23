using AutoMapper;
using CLS.BLL.DTOs.Payments;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        // Payment → PaymentResponse (flatten navigation properties)
        CreateMap<Payment, PaymentResponse>()
            .ForMember(d => d.Amount,          o => o.MapFrom(s => s.Amount))
            .ForMember(d => d.StudentName,     o => o.MapFrom(s => s.StudentPackage.Student.FullName))
            .ForMember(d => d.PackageName,     o => o.MapFrom(s => s.StudentPackage.Package.Name))
            .ForMember(d => d.RecordedByName,  o => o.MapFrom(s => s.RecordedBy.FullName));

        // StudentPackage → StudentPackageResponse (flatten Package name)
        CreateMap<StudentPackage, StudentPackageResponse>()
            .ForMember(d => d.PackageName, o => o.MapFrom(s => s.Package.Name));

        // TuitionPackage → TuitionPackageDto (straight mapping)
        CreateMap<TuitionPackage, TuitionPackageDto>();
    }
}
