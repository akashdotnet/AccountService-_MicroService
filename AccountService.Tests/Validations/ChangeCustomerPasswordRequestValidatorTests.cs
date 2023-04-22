using System.Threading.Tasks;
using AccountService.API.Dto.Request;
using AccountService.API.Validations;
using AutoFixture;
using FluentValidation.TestHelper;
using Xunit;

namespace AccountService.API.Tests.Validations;

public class ChangeCustomerPasswordRequestValidatorTests
{
    private readonly IFixture _fixture;
    private readonly ChangeCustomerPasswordRequestValidator _validator;

    public ChangeCustomerPasswordRequestValidatorTests()
    {
        _fixture = new Fixture();
        _validator = new ChangeCustomerPasswordRequestValidator();
    }


    [Fact(DisplayName =
        "ChangeCustomerPasswordRequestValidator : ChangeCustomerPasswordRequest - Should validate change password request successfully.")]
    public async Task ChangeCustomerPasswordRequestValidator_Success()
    {
        // arrange
        ChangeCustomerPasswordRequest changeCustomerPasswordMock = _fixture.Build<ChangeCustomerPasswordRequest>()
            .With(x => x.CurrentPassword, "current-password")
            .With(x => x.NewPassword, "new-password")
            .Create();


        // act
        TestValidationResult<ChangeCustomerPasswordRequest> result =
            await _validator.TestValidateAsync(changeCustomerPasswordMock);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName =
        "ChangeCustomerPasswordRequestValidator : ChangeCustomerPasswordRequest - Should have validation errors for New password field if its same as current password.")]
    public async Task ChangeCustomerPasswordRequestValidator_SameNewAndCurrentPassword_ThrowsValidationErrors()
    {
        // arrange
        ChangeCustomerPasswordRequest changeCustomerPasswordMock = _fixture.Build<ChangeCustomerPasswordRequest>()
            .With(x => x.CurrentPassword, "current-password")
            .With(x => x.NewPassword, "current-password")
            .Create();

        // act
        TestValidationResult<ChangeCustomerPasswordRequest>? result =
            await _validator.TestValidateAsync(changeCustomerPasswordMock);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
