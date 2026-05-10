using CLS.BLL.DTOs.Users;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ tên không vượt quá 100 ký tự.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không hợp lệ.");
        RuleFor(x => x.Phone).Matches(@"^\d{10,11}$").WithMessage("SĐT phải có 10-11 chữ số.")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
