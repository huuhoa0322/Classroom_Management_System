using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdateSessionRequestValidator : AbstractValidator<UpdateSessionRequest>
{
    public UpdateSessionRequestValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("ID lớp học phải lớn hơn 0.");

        RuleFor(x => x.TeacherId)
            .GreaterThan(0).WithMessage("ID giáo viên phải lớn hơn 0.");

        RuleFor(x => x.RoomId)
            .GreaterThan(0).WithMessage("ID phòng học phải lớn hơn 0.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu.");

        // Operating Hours: 08:00 - 21:00
        RuleFor(x => x.StartTime)
            .Must(BeWithinOperatingHours)
            .WithMessage($"Thời gian bắt đầu phải nằm trong khung giờ hoạt động ({AppConstants.OperatingHours.OpenHour}:00 - {AppConstants.OperatingHours.CloseHour}:00).");

        RuleFor(x => x.EndTime)
            .Must(BeWithinOperatingHours)
            .WithMessage($"Thời gian kết thúc phải nằm trong khung giờ hoạt động ({AppConstants.OperatingHours.OpenHour}:00 - {AppConstants.OperatingHours.CloseHour}:00).");
    }

    private static bool BeWithinOperatingHours(DateTime utcTime)
    {
        var vietnamTime = utcTime.AddHours(7);
        var hour = vietnamTime.Hour;
        return hour >= AppConstants.OperatingHours.OpenHour
            && hour < AppConstants.OperatingHours.CloseHour;
    }
}
