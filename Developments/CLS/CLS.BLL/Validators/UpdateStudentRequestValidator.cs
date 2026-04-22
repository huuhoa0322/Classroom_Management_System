using CLS.BLL.DTOs.Students;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên học sinh là bắt buộc.")
            .MaximumLength(255).WithMessage("Họ và tên không được vượt quá 255 ký tự.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || dob < DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Ngày sinh phải là ngày trong quá khứ.");
    }
}
