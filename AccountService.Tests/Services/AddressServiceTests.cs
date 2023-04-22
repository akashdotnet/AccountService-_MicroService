using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using Moq;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class AddressServiceTests
{
    private readonly IAddressService _addressService;
    private readonly IFixture _fixture;
    private readonly Mock<ICatalogServiceClient> _mockCatalogService;

    public AddressServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockCatalogService = new Mock<ICatalogServiceClient>();
        _addressService = new AddressService(_mockCatalogService.Object);
    }

    [Fact(DisplayName =
        "AddressService : ValidateAddress : Should validate address for state and zip-code successfully.")]
    public async Task Address_WithValidStateAndZipCode_ShouldValidateSuccessfully()
    {
        // arrange
        const string validState = "California";
        const string countyName = "Jefferson";
        const string zipCode = "35601";

        StateByZipCodeResponse stateByZipCodeDataCalifornia = _fixture.Build<StateByZipCodeResponse>()
            .With(x => x.Name, validState)
            .With(x => x.County, new CountyByZipCodeResponse {Name = countyName}).Create();

        CountyResponse countyData = _fixture.Build<CountyResponse>()
            .With(x => x.Name, countyName)
            .Without(x => x.ZipCodes)
            .Create();

        StateResponse stateData = _fixture.Build<StateResponse>()
            .With(x => x.Name, validState)
            .With(x => x.Counties, new List<CountyResponse> {countyData}).Create();

        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .With(x => x.City, countyName)
            .With(x => x.State, validState)
            .With(x => x.ZipCode, zipCode)
            .Create();

        _mockCatalogService.Setup(x => x.GetStates(false)).ReturnsAsync(new List<StateResponse> {stateData});
        _mockCatalogService.Setup(x => x.GetStateAndCountyByZipCode(It.IsAny<string>()))
            .ReturnsAsync(stateByZipCodeDataCalifornia);

        // act and assert
        Exception? exception = await Record.ExceptionAsync(() => _addressService.ValidateAddress(address));
        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddressService : ValidateAddress : Should throw validation error for invalid state.")]
    public async Task Address_WithInvalidState_ShouldThrowValidationErrors()
    {
        // arrange
        const string invalidState = "Bangalore";
        const string validState = "California";
        const string countyName = "Jefferson";
        const string zipCode = "35601";
        CountyResponse countyData = _fixture.Build<CountyResponse>()
            .With(x => x.Name, countyName)
            .Without(x => x.ZipCodes)
            .Create();

        StateResponse stateData = _fixture.Build<StateResponse>()
            .With(x => x.Name, validState)
            .With(x => x.Counties, new List<CountyResponse> {countyData}).Create();

        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .With(x => x.City, countyName)
            .With(x => x.State, invalidState)
            .With(x => x.ZipCode, zipCode)
            .Create();

        _mockCatalogService.Setup(x => x.GetStates(false)).ReturnsAsync(new List<StateResponse> {stateData});

        // act and assert
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _addressService.ValidateAddress(address));

        Assert.Equal(businessRuleViolationException.ErrorResponseType, StaticValues.InvalidStateName);
        Assert.Equal($"{invalidState} is not a valid state. Allowed states are: {validState}",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "AddressService : ValidateAddress : Should throw validation error for invalid zip code.")]
    public async Task Address_WithInvalidZipCode_ShouldThrowValidationError()
    {
        // arrange
        const string validState = "California";
        const string countyName = "Jefferson";
        const string zipCode = "35601";
        CountyResponse countyData = _fixture.Build<CountyResponse>()
            .With(x => x.Name, countyName)
            .Without(x => x.ZipCodes)
            .Create();

        StateResponse stateData = _fixture.Build<StateResponse>()
            .With(x => x.Name, validState)
            .With(x => x.Counties, new List<CountyResponse> {countyData}).Create();

        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .With(x => x.City, countyName)
            .With(x => x.State, validState)
            .With(x => x.ZipCode, zipCode)
            .Create();

        _mockCatalogService.Setup(x => x.GetStates(false)).ReturnsAsync(new List<StateResponse> {stateData});
        _mockCatalogService.Setup(x => x.GetStateAndCountyByZipCode(It.IsAny<string>()))
            .ReturnsAsync(new StateByZipCodeResponse());

        // act and assert
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _addressService.ValidateAddress(address));

        Assert.Equal(StaticValues.InvalidZipCode, businessRuleViolationException.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorInvalidZipCode, businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "AddressService : ValidateAddress : Should throw validation error for invalid state and zip code mapping.")]
    public async Task Address_WithInvalidStateAndZipCode_ShouldThrowValidationError()
    {
        // arrange
        const string validStateCalifornia = "California";
        const string validStateWashington = "Washington";
        const string countyName = "Jefferson";
        const string zipCode = "35601";
        CountyResponse countyData = _fixture.Build<CountyResponse>()
            .With(x => x.Name, countyName)
            .Without(x => x.ZipCodes)
            .Create();

        StateByZipCodeResponse stateByZipCodeDataCalifornia = _fixture.Build<StateByZipCodeResponse>()
            .With(x => x.Name, validStateCalifornia)
            .With(x => x.County, new CountyByZipCodeResponse {Name = countyName}).Create();

        StateResponse stateDataCalifornia = _fixture.Build<StateResponse>()
            .With(x => x.Name, validStateCalifornia)
            .With(x => x.Counties, new List<CountyResponse> {countyData}).Create();
        StateResponse stateDataWashington = _fixture.Build<StateResponse>()
            .With(x => x.Name, validStateWashington)
            .With(x => x.Counties, new List<CountyResponse> {countyData}).Create();

        CustomerAddressRequest address = _fixture.Build<CustomerAddressRequest>()
            .With(x => x.City, countyName)
            .With(x => x.State, validStateWashington)
            .With(x => x.ZipCode, zipCode)
            .Create();

        _mockCatalogService.Setup(x => x.GetStates(false)).ReturnsAsync(new List<StateResponse>
            {stateDataCalifornia, stateDataWashington});
        _mockCatalogService.Setup(x => x.GetStateAndCountyByZipCode(It.IsAny<string>()))
            .ReturnsAsync(stateByZipCodeDataCalifornia);

        // act and assert
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _addressService.ValidateAddress(address));

        Assert.Equal(StaticValues.InvalidZipCodeForSelectedState, businessRuleViolationException.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorInvalidZipCodeForSelectedState, businessRuleViolationException.Message);
    }
}
