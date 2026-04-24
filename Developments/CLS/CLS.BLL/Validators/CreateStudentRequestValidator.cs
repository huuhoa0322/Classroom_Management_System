using CLS.BLL.DTOs.Students;
using FluentValidation;

namespace CLS.BLL.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        // ── Thông tin học sinh ───────────────────────────────────────────
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên học sinh là bắt buộc.")
            .MaximumLength(255).WithMessage("Họ và tên không được vượt quá 255 ký tự.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || dob < DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Ngày sinh phải là ngày trong quá khứ.");

        // ── Thông tin phụ huynh ──────────────────────────────────────────
        RuleFor(x => x.ParentEmail)
            .NotEmpty().WithMessage("Email phụ huynh là bắt buộc để hệ thống gửi thông báo tự động.")
            .EmailAddress().WithMessage("Email phụ huynh không đúng định dạng.");

        RuleFor(x => x.ParentFullName)
            .NotEmpty().WithMessage("Họ và tên phụ huynh là bắt buộc.")
            .MaximumLength(255).WithMessage("Họ và tên phụ huynh không được vượt quá 255 ký tự.");

        RuleFor(x => x.ParentPhone)
            .Matches(@"^\d{10,11}$")
            .When(x => !string.IsNullOrEmpty(x.ParentPhone))
            .WithMessage("Số điện thoại phụ huynh phải có 10-11 chữ số.");

        RuleFor(x => x.ParentRelationship)
            .NotEmpty().WithMessage("Quan hệ với học sinh là bắt buộc.")
            .MaximumLength(50).WithMessage("Quan hệ không được vượt quá 50 ký tự.");
    }
}
