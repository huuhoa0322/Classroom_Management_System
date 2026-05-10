using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.StudentPackages;
using CLS.DAL.Entities;

namespace CLS.BLL.Mappings;

public class PackageMappingProfile : Profile
{
    public PackageMappingProfile()
    {
        CreateMap<TuitionPackage, PackageResponse>()
            .ForMember(d => d.StudentCount,
                       o => o.MapFrom(s => s.StudentPackages.Count));
    }
}
