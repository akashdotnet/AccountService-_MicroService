using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Models;

namespace AccountService.API.Services.Interfaces;

public interface IDealerService
{
    Task<DealerResponse> CreateDealer(UserInformationResponse userInformation);
    Task<Dealer> UpdateDealer(UpdateDealerRequest updateDealerRequest, string userType);
    Task<DealerResponse> GetDealerWithBusinessLogo();

    Task<List<DealerResponse>> GetDealerByBusinessLocationIds(List<int> businessLocationIds,
        bool? isOnboardingComplete = null);

    Task<DealerLocationProfileResponse> GetDealerLocationProfile(int businessLocationId);
    Task<(Dealer, BusinessLocation)> GetDealerAndBusinessLocationById(int businessLocationId);

    Task<DealerLocationProfileResponse> MapDealerAndBusinessLocationToDealerLocationResponse(
        Dealer dealer,
        BusinessLocation businessLocation);

    Task<List<DealerProfileResponse>> GetDealersBySearchStringAndState(string businessSearchString, string state);

    Task<List<DealerResponse>> GetDealersByMatchCriteria(string zipcode, List<string> jobCategoryCodes,
        bool isOnboardingComplete);

    Task<DealerTermsAndConditionsResponse> UpdateDealerTermsAndConditions(
        DealerTermsAndConditionsRequest dealerTermsAndConditionsRequest);
}
