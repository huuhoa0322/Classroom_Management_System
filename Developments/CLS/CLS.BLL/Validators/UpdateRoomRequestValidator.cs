using CLS.BLL.DTOs.Rooms;
using FluentValidation;

namespace CLS.BLL.Validators;

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên phòng không được để trống.")
            .MaximumLength(100).WithMessage("Tên phòng không vượt quá 100 ký tự.");
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Sức chứa phải lớn hơn 0.");
    }
}
