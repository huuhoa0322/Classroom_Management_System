using CLS.BLL.Common;
using CLS.BLL.DTOs.Payments;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdatePaymentStatusRequestValidator : AbstractValidator<UpdatePaymentStatusRequest>
{
    private static readonly string[] ValidStatuses =
    [
        AppConstants.PaymentStatus.Confirmed,
        AppConstants.PaymentStatus.Failed,
        AppConstants.PaymentStatus.Refunded,
    ];

    public UpdatePaymentStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái là bắt buộc.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Trạng thái phải là một trong: {string.Join(", ", ValidStatuses)}.");
    }
}
