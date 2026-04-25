using CLS.BLL.Common;
using CLS.BLL.DTOs.RenewalAlerts;
using FluentValidation;

namespace CLS.BLL.Validators;

/// <summary>
/// Validator cho UpdateAlertStatusRequest — đảm bảo status hợp lệ.
/// </summary>
public class UpdateAlertStatusRequestValidator : AbstractValidator<UpdateAlertStatusRequest>
{
    private static readonly string[] ValidStatuses =
    [
        AppConstants.AlertNotificationStatus.Pending,
        AppConstants.AlertNotificationStatus.Consulted
    ];

    public UpdateAlertStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái không được để trống.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Trạng thái phải là một trong: {string.Join(", ", ValidStatuses)}.");
    }
}
