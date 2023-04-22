using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Services.Interfaces;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class AddressService : IAddressService
{
    private readonly ICatalogServiceClient _catalogServiceClient;

    public AddressService(ICatalogServiceClient catalogServiceClient)
    {
        _catalogServiceClient = catalogServiceClient ?? throw new ArgumentNullException(nameof(catalogServiceClient));
    }

    public async Task ValidateAddress(CustomerAddressRequest customerAddressRequest)
    {
        List<StateResponse> stateResponseList = await _catalogServiceClient.GetStates();

        // 1. Validate State
        if (stateResponseList.FirstOrDefault(x => x.Name.Equals(customerAddressRequest.State)) == null)
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidStateName,
                StaticValues.ErrorInvalidState(customerAddressRequest.State,
                    stateResponseList.Select(s => s.Name).ToArray()));
        }

        // 2. Validate state and zip-code mapping
        StateByZipCodeResponse stateResponse =
            await _catalogServiceClient.GetStateAndCountyByZipCode(customerAddressRequest.ZipCode);
        if (string.IsNullOrWhiteSpace(stateResponse.Name))
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidZipCode, StaticValues.ErrorInvalidZipCode);
        }

        if (!stateResponse.Name.Equals(customerAddressRequest.State))
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidZipCodeForSelectedState,
                StaticValues.ErrorInvalidZipCodeForSelectedState);
        }
    }
}
