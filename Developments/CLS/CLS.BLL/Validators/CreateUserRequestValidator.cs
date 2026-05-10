using CLS.BLL.DTOs.Users;
using FluentValidation;

namespace CLS.BLL.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ tên không vượt quá 100 ký tự.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không hợp lệ.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");
        RuleFor(x => x.Phone).Matches(@"^\d{10,11}$").WithMessage("SĐT phải có 10-11 chữ số.")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
