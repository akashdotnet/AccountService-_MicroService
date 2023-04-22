using System.Collections.Generic;
using System.Linq;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class ValidationService
{
    public static void ValidateBrandCode(string brandCode, List<BrandResponse> brandResponses)
    {
        string[] allowedBrandCodes = brandResponses.Select(b => b.Code).ToArray();
        if (!allowedBrandCodes.Contains(brandCode))
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidBrandCode,
                StaticValues.ErrorInvalidBrandCode(brandCode, allowedBrandCodes));
        }
    }

    public static void ValidateBusinessLocation(BusinessLocationRequest businessLocation,
        List<StateResponse> stateResponses)
    {
        StateResponse? stateResponse = stateResponses.Find(s => s.Name == businessLocation.Address.State);

        // 1. Validate State
        if (stateResponse == null)
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidStateName,
                StaticValues.ErrorInvalidState(businessLocation.Address.State,
                    stateResponses.Select(s => s.Name).ToArray()));
        }

        Dictionary<string, CountyResponse> countyNameAndCountyResponseMap =
            stateResponse.Counties.ToDictionary(countyResponse => countyResponse.Name,
                countyResponse => countyResponse);

        // 2. Validate Counties for the selected state
        List<string> invalidCounties = (businessLocation.ServiceableCounties ?? new List<string>())
            .Aggregate(new List<string>(),
                (errorCountiesAccumulator, serviceableCounty) =>
                {
                    if (!countyNameAndCountyResponseMap.ContainsKey(serviceableCounty))
                    {
                        errorCountiesAccumulator.Add(serviceableCounty);
                    }

                    return errorCountiesAccumulator;
                });

        if (invalidCounties.Count != 0)
        {
            throw new BusinessRuleViolationException(
                StaticValues.InvalidCounties,
                StaticValues.ErrorInvalidCounties(
                    invalidCounties,
                    countyNameAndCountyResponseMap.Keys.ToList(),
                    businessLocation.Address.State));
        }

        // 3. Validate the zip code for the selected state
        ValidateZipCode(stateResponse, businessLocation.Address.ZipCode);
    }

    private static void ValidateZipCode(StateResponse stateResponse, string zipCode)
    {
        List<string> validZipCodes = stateResponse.Counties.Aggregate(new List<string>(),
            (zipCodeAccumulator, countyResponse) =>
            {
                zipCodeAccumulator.AddRange(countyResponse.ZipCodes);
                return zipCodeAccumulator;
            });
        if (!validZipCodes.Contains(zipCode))
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidZipCode,
                StaticValues.ErrorInvalidZipCodeForState(zipCode,
                    validZipCodes));
        }
    }
}
