using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using FluentValidation;

namespace AccountService.API.Validations;

public class ChangeCustomerPasswordRequestValidator : AbstractValidator<ChangeCustomerPasswordRequest>
{
    public ChangeCustomerPasswordRequestValidator()
    {
        RuleFor(changeCustomerPasswordRequest => changeCustomerPasswordRequest.NewPassword)
            .NotEqual(changeCustomerPasswordRequest => changeCustomerPasswordRequest.CurrentPassword)
            .WithMessage(StaticValues.ErrorSameCurrentAndNewPassword)
            .NotNull();
    }
}
