using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Enums;
using AccountService.API.Validations;
using AutoFixture;
using AutoFixture.DataAnnotations;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace AccountService.API.Tests.Validations;

public class UpdateDealerRequestValidatorTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICatalogServiceClient> _mockCatalogServiceClient;
    private readonly UpdateDealerRequestValidator _validator;

    public UpdateDealerRequestValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _mockCatalogServiceClient = new Mock<ICatalogServiceClient>();
        _validator = new UpdateDealerRequestValidator(_mockCatalogServiceClient.Object);
    }

    [Fact(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should successfully validate dealer on-boarding request for AboutBusiness step.")]
    public void UpdateDealerRequestValidator_AboutBusiness_Success()
    {
        // arrange - about is not a required field for AboutBusiness step
        const string phoneNumber = "1234567890";
        const string websiteUrl = "www.pentair.com";
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.AboutBusiness)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .Without(x => x.About)
                .With(x => x.PhoneNumber, phoneNumber)
                .With(x => x.WebsiteUrl, websiteUrl)
                .Create()
            )
            .Create();

        // act
        TestValidationResult<UpdateDealerRequest>? result = _validator.TestValidate(updateDealerRequest);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should throw validation errors when required field locations are not provided for AboutBusiness step.")]
    public void UpdateDealerRequestValidator_AboutBusiness_ThrowsValidationErrors()
    {
        // arrange
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.AboutBusiness)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .Without(x => x.Locations)
                .Create()
            )
            .Create();

        // act
        TestValidationResult<UpdateDealerRequest>? result = _validator.TestValidate(updateDealerRequest);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Business.Locations);
    }

    [Theory(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should successfully validate website url for dealer on-boarding request while filling info in AboutBusiness step.")]
    [InlineData("pentair.com")]
    [InlineData("www.pentair.com")]
    [InlineData("http://www.pentair.com")]
    [InlineData("https://www.pentair.com")]
    public void UpdateDealerRequestValidator_WhenValidWebsiteUrlIsProvided_SuccessfullyValidatesTheWebsiteUrl(
        string websiteUrl)
    {
        // arrange - about is not a required field for AboutBusiness step
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.AboutBusiness)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .Without(x => x.About)
                .With(x => x.WebsiteUrl, websiteUrl)
                .Create()
            )
            .Create();

        // act
        TestValidationResult<UpdateDealerRequest>? result = _validator.TestValidate(updateDealerRequest);

        // assert
        Assert.True(result.IsValid);
        result.ShouldNotHaveValidationErrorFor(x => x.Business.WebsiteUrl);
    }

    [Theory(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should throw validation errors for invalid website url for dealer on-boarding request while filling info in AboutBusiness step.")]
    [InlineData("www.pentair")]
    [InlineData("wwwpentair")]
    [InlineData("wwwpentaircom")]
    [InlineData("pentair")]
    [InlineData("pentair.")]
    public void UpdateDealerRequestValidator_WhenInvalidWebsiteUrlIsProvided_ThrowsValidationErrors(string websiteUrl)
    {
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .With(x => x.WebsiteUrl, websiteUrl)
                .Create()
            )
            .Create();
        _mockCatalogServiceClient.Setup(x => x.GetJobCategories()).ReturnsAsync(GetJobCategories());

        // act
        TestValidationResult<UpdateDealerRequest>? result = _validator.TestValidate(updateDealerRequest);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Business.WebsiteUrl);
        Assert.Contains(result.Errors, x => x.ErrorMessage == StaticValues.InvalidWebSiteUrl);
    }

    [Fact(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should successfully validate dealer on-boarding request for PublicCompanyProfile step.")]
    public void UpdateDealerRequestValidator_PublicCompanyProfile_Success()
    {
        // arrange
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .Without(x => x.Locations)
                .Create()
            )
            .Create();

        // act
        TestValidationResult<UpdateDealerRequest>? result = _validator.TestValidate(updateDealerRequest);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should throw validation errors when required fields are not provided for PublicCompanyProfile step.")]
    public async Task UpdateDealerRequestValidator_PublicCompanyProfile_ThrowsValidationErrors()
    {
        // arrange
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .Without(x => x.About)
                .Without(x => x.Categories)
                .Without(x => x.Brands)
                .Without(x => x.PoolCount)
                .Without(x => x.StartYear)
                .Without(x => x.PhoneNumber)
                .Create()
            )
            .Create();
        _mockCatalogServiceClient.Setup(x => x.GetJobCategories()).ReturnsAsync(GetJobCategories());

        // act
        TestValidationResult<UpdateDealerRequest>? result = await _validator.TestValidateAsync(updateDealerRequest);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Business.About);
        result.ShouldHaveValidationErrorFor(x => x.Business.Brands);
        result.ShouldHaveValidationErrorFor(x => x.Business.Categories);
        result.ShouldHaveValidationErrorFor(x => x.Business.PoolCount);
        result.ShouldHaveValidationErrorFor(x => x.Business.StartYear);
        result.ShouldHaveValidationErrorFor(x => x.Business.PhoneNumber);
    }

    [Fact(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should throw validation errors when job categories are invalid for PublicCompanyProfile step.")]
    public async Task UpdateDealerRequestValidator_PublicCompanyProfile_JobCategoriesInvalid_ThrowsValidationErrors()
    {
        // arrange
        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .With(x => x.Business, _fixture.Build<BusinessRequest>().Create())
            .Create();
        _mockCatalogServiceClient.Setup(x => x.GetJobCategories()).ReturnsAsync(GetJobCategories());

        // act
        TestValidationResult<UpdateDealerRequest>? result = await _validator.TestValidateAsync(updateDealerRequest);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Business.Categories);
    }

    [Fact(DisplayName =
        "UpdateDealerRequestValidator: ValidateUpdateDealerRequest - Should validate successfully when update dealer request is valid for PublicCompanyProfile step.")]
    public async Task UpdateDealerRequestValidator_PublicCompanyProfileStep_Success()
    {
        // arrange
        JobCategoriesRequest? incomingRequestCategoryOne = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Codes, new List<string> {"pump_repair"})
            .Create();
        JobCategoriesRequest? incomingRequestCategoryTwo = _fixture.Build<JobCategoriesRequest>()
            .With(x => x.Codes, new List<string> {"free_assessment"})
            .Create();
        List<JobCategoriesRequest> incomingJobCategoriesRequest =
            new() {incomingRequestCategoryOne, incomingRequestCategoryTwo};

        UpdateDealerRequest? updateDealerRequest = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .With(x => x.Categories, incomingJobCategoriesRequest)
                .With(x => x.WebsiteUrl, "https://www.pentair.com")
                .With(x => x.PhoneNumber, "123456789")
                .Create())
            .Create();
        _mockCatalogServiceClient.Setup(x => x.GetJobCategories()).ReturnsAsync(GetJobCategories());

        // act
        TestValidationResult<UpdateDealerRequest>? result = await _validator.TestValidateAsync(updateDealerRequest);

        // assert
        Assert.True(result.IsValid);
        result.ShouldNotHaveValidationErrorFor(x => x.Business.Categories);
    }

    private List<JobCategoryResponse> GetJobCategories()
    {
        JobCategoryResponse? incomingCategoryOne = _fixture.Build<JobCategoryResponse>()
            .With(x => x.Code, "pump_repair")
            .Create();
        JobCategoryResponse? incomingCategoryTwo = _fixture.Build<JobCategoryResponse>()
            .With(x => x.Code, "free_assessment")
            .Create();
        return new List<JobCategoryResponse> {incomingCategoryOne, incomingCategoryTwo};
    }
}
