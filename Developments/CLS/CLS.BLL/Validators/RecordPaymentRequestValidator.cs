using CLS.BLL.DTOs.Payments;
using FluentValidation;

namespace CLS.BLL.Validators;

public class RecordPaymentRequestValidator : AbstractValidator<RecordPaymentRequest>
{
    public RecordPaymentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("ID học sinh phải lớn hơn 0.");

        RuleFor(x => x.TuitionPackageId)
            .GreaterThan(0).WithMessage("ID gói học phải lớn hơn 0.");

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0).WithMessage("Số tiền thanh toán phải >= 0.");
    }
}
