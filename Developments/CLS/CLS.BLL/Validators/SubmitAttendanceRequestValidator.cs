using CLS.BLL.Common;
using CLS.BLL.DTOs.Attendance;
using FluentValidation;

namespace CLS.BLL.Validators;

public class SubmitAttendanceRequestValidator : AbstractValidator<SubmitAttendanceRequest>
{
    private static readonly string[] ValidStatuses =
    [
        AppConstants.AttendanceStatus.Present,
        AppConstants.AttendanceStatus.Absent,
        AppConstants.AttendanceStatus.Late
    ];

    public SubmitAttendanceRequestValidator()
    {
        RuleFor(x => x.Records)
            .NotEmpty()
            .WithMessage("Danh sách điểm danh không được để trống.");

        RuleForEach(x => x.Records).ChildRules(record =>
        {
            record.RuleFor(r => r.StudentId)
                  .GreaterThan(0)
                  .WithMessage("StudentId phải lớn hơn 0.");

            record.RuleFor(r => r.Status)
                  .Must(s => ValidStatuses.Contains(s))
                  .WithMessage($"Status phải là một trong: {string.Join(", ", ValidStatuses)}.");
        });
    }
}
