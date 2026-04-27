using CLS.BLL.DTOs.Feedback;
using FluentValidation;

namespace CLS.BLL.Validators;

public class SubmitFeedbackRequestValidator : AbstractValidator<SubmitFeedbackRequest>
{
    public SubmitFeedbackRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("StudentId không hợp lệ.");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 10)
            .WithMessage("Điểm đánh giá phải từ 1 đến 10.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung nhận xét không được để trống.")
            .MaximumLength(1000)
            .WithMessage("Nội dung nhận xét không được vượt quá 1000 ký tự.");
    }
}
