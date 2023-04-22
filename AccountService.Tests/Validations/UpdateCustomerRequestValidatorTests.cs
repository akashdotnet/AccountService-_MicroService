using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Enums;
using AccountService.API.Validations;
using AutoFixture;
using AutoFixture.DataAnnotations;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace AccountService.API.Tests.Validations;

public class UpdateCustomerRequestValidatorTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICatalogServiceClient> _mockCatalogServiceClient;
    private readonly UpdateCustomerRequestValidator _validator;

    public UpdateCustomerRequestValidatorTests()
    {
        _fixture = new Fixture();
        //add in the ordered customizations (put derived ones before base ones)
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _mockCatalogServiceClient = new Mock<ICatalogServiceClient>();
        _validator = new UpdateCustomerRequestValidator(_mockCatalogServiceClient.Object);
        SetUpMockServices();
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should validate customer successfully for on-boarding.")]
    public async Task ShouldPassValidations_WhenCustomerOnBoardingStepIsGettingStarted()
    {
        // arrange
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.GettingStarted)
            .With(x => x.PoolMaterialCode, string.Empty)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation error when address is not provided in customer on-boarding.")]
    public async Task ShouldThrowValidations_WhenCustomerAddressNotProvidedInCustomerOnBoarding()
    {
        // arrange
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.GettingStarted)
            .Without(x => x.Address)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation error when zip-code is not provided in customer on-boarding.")]
    public async Task ShouldThrowValidations_WhenCustomerZipCodeNotProvidedInCustomerOnBoarding()
    {
        // arrange
        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .Without(x => x.ZipCode)
            .Create();
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.GettingStarted)
            .With(x => x.Address, address)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Address.ZipCode);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation error when city is not provided in customer on-boarding.")]
    public async Task ShouldThrowValidations_WhenCustomerCityNotProvidedInCustomerOnBoarding()
    {
        // arrange
        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .Without(x => x.City)
            .Create();
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.GettingStarted)
            .With(x => x.Address, address)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Address.City);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation error when state is not provided in customer on-boarding.")]
    public async Task ShouldThrowValidations_WhenCustomerStateNotProvidedInCustomerOnBoarding()
    {
        // arrange
        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .Without(x => x.State)
            .Create();
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.GettingStarted)
            .With(x => x.Address, address)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Address.State);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation error when address value is not provided in customer on-boarding.")]
    public async Task ShouldThrowValidations_WhenCustomerAddressValueNotProvidedInCustomerOnBoarding()
    {
        // arrange
        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .Without(x => x.AddressValue)
            .Create();
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.GettingStarted)
            .With(x => x.Address, address)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.Address.AddressValue);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation errors for all fields for customer on-boarding during pool details step.")]
    public async Task WhenCustomerOnBoardingStepIsPoolDetails_InvalidModel_ThrowsValidationErrors()
    {
        // arrange
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.PoolDetails)
            .Create();

        // act
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert
        Assert.False(result.IsValid);
        result.ShouldHaveValidationErrorFor(x => x.PoolSizeCode);
        result.ShouldHaveValidationErrorFor(x => x.PoolTypeCode);
        result.ShouldHaveValidationErrorFor(x => x.PoolMaterialCode);
        result.ShouldHaveValidationErrorFor(x => x.HotTubTypeCode);
        result.ShouldHaveValidationErrorFor(x => x.SanitationMethodCode);
        result.ShouldHaveValidationErrorFor(x => x.PoolSeasonCode);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should have validation errors for some fields for customer on-boarding during pool details step.")]
    public async Task WhenCustomerOnBoardingStepIsPoolDetails_PartiallyInvalidModel_ThrowsValidationErrors()
    {
        // arrange                                                                                       
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.PoolDetails)
            .With(x => x.PoolSizeCode, "medium")
            .Create();

        // act                                                                                           
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert                                                                                        
        Assert.False(result.IsValid);
        result.ShouldNotHaveValidationErrorFor(x => x.PoolSizeCode);
        result.ShouldHaveValidationErrorFor(x => x.PoolTypeCode);
        result.ShouldHaveValidationErrorFor(x => x.PoolMaterialCode);
        result.ShouldHaveValidationErrorFor(x => x.HotTubTypeCode);
        result.ShouldHaveValidationErrorFor(x => x.SanitationMethodCode);
        result.ShouldHaveValidationErrorFor(x => x.PoolSeasonCode);
    }

    [Fact(DisplayName =
        "UpdateCustomerRequestValidator : ValidateUpdateCustomerRequest - Should validate customer successfully for on-boarding during pool details step.")]
    public async Task WhenCustomerOnBoardingStepIsPoolDetails_ValidModel_ValidationSuccess()
    {
        // arrange                                                                                                   
        UpdateCustomerRequest? updateCustomerRequestObj = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.PoolDetails)
            .With(x => x.PoolSizeCode, "medium")
            .With(x => x.PoolTypeCode, "lap_pool")
            .With(x => x.SanitationMethodCode, "water_pumps")
            .With(x => x.PoolSeasonCode, "open_closed_seasons")
            .With(x => x.PoolMaterialCode, "fibreglass")
            .With(x => x.HotTubTypeCode, "none")
            .Create();

        // act                                                                                                       
        TestValidationResult<UpdateCustomerRequest>? result =
            await _validator.TestValidateAsync(updateCustomerRequestObj);

        // assert                                                                                                    
        Assert.True(result.IsValid);
        result.ShouldNotHaveValidationErrorFor(x => x.PoolSizeCode);
        result.ShouldNotHaveValidationErrorFor(x => x.PoolTypeCode);
        result.ShouldNotHaveValidationErrorFor(x => x.HotTubTypeCode);
        result.ShouldNotHaveValidationErrorFor(x => x.PoolMaterialCode);
        result.ShouldNotHaveValidationErrorFor(x => x.SanitationMethodCode);
        result.ShouldNotHaveValidationErrorFor(x => x.PoolSeasonCode);
    }

    private void SetUpMockServices()
    {
        PoolDetailLookups poolDetailLookups = new();

        PoolSizeResponse? poolSizeCode = _fixture.Build<PoolSizeResponse>()
            .With(x => x.Code, "medium").Create();
        poolDetailLookups.PoolSizes.Add(poolSizeCode);

        PoolMaterialResponse? poolMaterialCode = _fixture.Build<PoolMaterialResponse>()
            .With(x => x.Code, "fibreglass").Create();
        poolDetailLookups.PoolMaterials.Add(poolMaterialCode);

        HotTubTypeResponse? hotTubTypeCode = _fixture.Build<HotTubTypeResponse>()
            .With(x => x.Code, "none").Create();
        poolDetailLookups.HotTubTypes.Add(hotTubTypeCode);

        PoolTypeResponse? poolTypeCode = _fixture.Build<PoolTypeResponse>()
            .With(x => x.Code, "lap_pool").Create();
        poolDetailLookups.PoolTypes.Add(poolTypeCode);

        SanitationMethodResponse? sanitationMethodCode = _fixture.Build<SanitationMethodResponse>()
            .With(x => x.Code, "water_pumps").Create();
        poolDetailLookups.SanitationMethods.Add(sanitationMethodCode);

        PoolSeasonResponse? poolSeasonCode = _fixture.Build<PoolSeasonResponse>()
            .With(x => x.Code, "open_closed_seasons").Create();
        poolDetailLookups.PoolSeasons.Add(poolSeasonCode);

        _mockCatalogServiceClient.Setup(x => x.GetPoolDetailLookups()).ReturnsAsync(poolDetailLookups);
    }
}
