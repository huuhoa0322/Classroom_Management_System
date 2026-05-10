using CLS.BLL.DTOs.Packages;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdatePackageRequestValidator : AbstractValidator<UpdatePackageRequest>
{
    public UpdatePackageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên gói không được để trống.")
            .MaximumLength(100).WithMessage("Tên gói không vượt quá 100 ký tự.");
        RuleFor(x => x.TotalSessions).GreaterThan(0).WithMessage("Số buổi phải lớn hơn 0.");
        RuleFor(x => x.DurationDays).GreaterThan(0).WithMessage("Thời hạn (ngày) phải lớn hơn 0.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Giá gói phải lớn hơn 0.");
    }
}
