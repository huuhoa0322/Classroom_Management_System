using CLS.BLL.DTOs.Classes;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdateClassRequestValidator : AbstractValidator<UpdateClassRequest>
{
    public UpdateClassRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên lớp không được để trống.")
            .MaximumLength(100).WithMessage("Tên lớp không được vượt quá 100 ký tự.");
    }
}
