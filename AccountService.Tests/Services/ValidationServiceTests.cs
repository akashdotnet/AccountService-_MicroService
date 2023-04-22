using System;
using System.Collections.Generic;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Services;
using AutoFixture;
using AutoFixture.DataAnnotations;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class ValidationServiceTests
{
    private const string ValidBrandCode = "valid_brand_code";
    private readonly IFixture _fixture;

    public ValidationServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());

        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact(DisplayName =
        "Should throw BusinessRuleViolationException with type InvalidBrandCode when the brand code provided in the input is invalid")]
    public void ValidateBrandCode_InvalidBrandCode()
    {
        Exception? exception = Record.Exception(
            () => ValidationService.ValidateBrandCode(ValidBrandCode, GetBrandResponsesMock())
        );

        Assert.Null(exception);
    }

    [Fact(DisplayName = "Should not throw any exceptions when valid brand codes are provided")]
    public void ValidateBrandCode_Success()
    {
        const string invalidBrandCode = "invalid_brand_code";
        BusinessRuleViolationException businessRuleViolationException = Assert.Throws<BusinessRuleViolationException>(
            () => ValidationService.ValidateBrandCode(invalidBrandCode, GetBrandResponsesMock()));

        Assert.IsType<BusinessRuleViolationException>(businessRuleViolationException);
        Assert.Equal(StaticValues.InvalidBrandCode, businessRuleViolationException.ErrorResponseType);
        Assert.Equal($"{invalidBrandCode} is not a valid brand code. Allowed brand codes are: {ValidBrandCode}",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "Should throw BusinessRuleViolationException with type InvalidStateName when the state provided in the input is invalid")]
    public void ValidateBusinessLocation_InvalidState()
    {
        const string invalidState = "InvalidState";
        BusinessLocationRequest businessLocationMock = _fixture.Build<BusinessLocationRequest>()
            .With(x => x.Address, _fixture.Build<DealerAddressRequest>()
                .With(x => x.State, invalidState)
                .Create()
            )
            .Create();

        BusinessRuleViolationException businessRuleViolationException = Assert.Throws<BusinessRuleViolationException>(
            () => ValidationService.ValidateBusinessLocation(businessLocationMock, GetStateResponseMock()));


        Assert.Equal(businessRuleViolationException.ErrorResponseType, StaticValues.InvalidStateName);
        Assert.Equal($"{invalidState} is not a valid state. Allowed states are: Alabama",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "Should throw BusinessRuleViolationException with type InvalidCounties when the counties given are not part of selected states")]
    public void ValidateBusinessLocation_InvalidCounties()
    {
        BusinessLocationRequest businessLocationMock = _fixture.Build<BusinessLocationRequest>()
            .With(x => x.Address, _fixture.Build<DealerAddressRequest>()
                .With(x => x.State, "Alabama")
                .Create()
            )
            .With(x => x.ServiceableCounties, new List<string> {"invalid_county_1", "invalid_county_2"})
            .Create();

        BusinessRuleViolationException businessRuleViolationException = Assert.Throws<BusinessRuleViolationException>(
            () => ValidationService.ValidateBusinessLocation(businessLocationMock, GetStateResponseMock())
        );

        Assert.Equal(businessRuleViolationException.ErrorResponseType, StaticValues.InvalidCounties);
        Assert.Equal(
            "invalid_county_1,invalid_county_2 are not valid counties. For state: Alabama allowed counties are: Autauga County",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "Should throw BusinessRuleViolationException with type InvalidZipCode when the zipcode is not part of selected counties")]
    public void ValidateBusinessLocation_InvalidZipCode()
    {
        BusinessLocationRequest businessLocationMock = _fixture.Build<BusinessLocationRequest>()
            .With(x => x.Address, _fixture.Build<DealerAddressRequest>()
                .With(x => x.State, "Alabama")
                .With(x => x.ZipCode, "76433")
                .Create()
            )
            .With(x => x.ServiceableCounties, new List<string> {"Autauga County"})
            .Create();

        BusinessRuleViolationException businessRuleViolationException = Assert.Throws<BusinessRuleViolationException>(
            () => ValidationService.ValidateBusinessLocation(businessLocationMock, GetStateResponseMock()));

        Assert.Equal(businessRuleViolationException.ErrorResponseType, StaticValues.InvalidZipCode);
        Assert.Equal("76433 is not a valid zip code for the selected state. Valid Zip codes are: 36006",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "Should not throw any exception if valid state, counties and zipcode is provided")]
    public void ValidateBusinessLocation_Success()
    {
        BusinessLocationRequest businessLocationMock = _fixture.Build<BusinessLocationRequest>()
            .With(x => x.Address, _fixture.Build<DealerAddressRequest>()
                .With(x => x.State, "Alabama")
                .With(x => x.ZipCode, "36006")
                .Create()
            )
            .With(x => x.ServiceableCounties, new List<string> {"Autauga County"})
            .Create();

        Exception? exception = Record.Exception(() =>
            ValidationService.ValidateBusinessLocation(businessLocationMock, GetStateResponseMock()));
        Assert.Null(exception);
    }

    private static List<BrandResponse> GetBrandResponsesMock()
    {
        return new List<BrandResponse>
        {
            new()
            {
                Code = ValidBrandCode
            }
        };
    }

    private static List<StateResponse> GetStateResponseMock()
    {
        return new List<StateResponse>
        {
            new()
            {
                Name = "Alabama",
                Counties = new List<CountyResponse>
                {
                    new()
                    {
                        Name = "Autauga County",
                        ZipCodes = new List<string> {"36006"}
                    }
                }
            }
        };
    }
}
