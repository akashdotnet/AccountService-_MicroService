using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;
using StackExchange.Redis;

namespace AccountService.API.Services;

public class DealerService : IDealerService
{
    private readonly IAccountService _accountService;
    private readonly IBusinessLocationServiceableCountyRepository _businessLocationServiceableCountyRepository;
    private readonly IBusinessService _businessService;
    private readonly IDistributedCache _cache;
    private readonly ICatalogServiceClient _catalogServiceClient;
    private readonly IDealerRepository _dealerRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IServer _redisServer;

    public DealerService(
        IAccountService accountService,
        IBusinessService businessService,
        IDealerRepository dealerRepository,
        IBusinessLocationServiceableCountyRepository businessLocationServiceableCountyRepository,
        ICatalogServiceClient catalogServiceClient,
        IMapper mapper,
        IDistributedCache cache,
        IServer redisServer,
        IPaymentServiceClient paymentServiceClient,
        INotificationService notificationService
    )
    {
        _accountService = accountService;
        _businessService = businessService;
        _dealerRepository = dealerRepository;
        _catalogServiceClient = catalogServiceClient;
        _mapper = mapper;
        _businessLocationServiceableCountyRepository = businessLocationServiceableCountyRepository;
        _cache = cache;
        _redisServer = redisServer;
        _notificationService = notificationService;
        _paymentServiceClient = paymentServiceClient;
    }

    public async Task<DealerResponse> CreateDealer(UserInformationResponse userInformation)
    {
        Account account = _mapper.Map<Account>(userInformation);
        account.UserRole = UserRoleEnum.Dealer;
        Dealer dealer = new()
        {
            Account = account,
            LastCompletedOnboardingStep = DealerStepEnum.SignUpComplete
        };
        Dealer createdDealer = await _dealerRepository.CreateDealer(dealer);
        return _mapper.Map<DealerResponse>(createdDealer);
    }

    public async Task<Dealer> UpdateDealer(UpdateDealerRequest updateDealerRequest, string userType)
    {
        Dealer dealer = await GetDealer();
        if (dealer.Business == null && updateDealerRequest.Business.Name.IsNullOrEmpty())
        {
            throw new BusinessRuleViolationException(StaticValues.RequiredBusinessName,
                StaticValues.ErrorRequiredBusinessName);
        }

        dealer.Business = await _businessService.CreateOrUpdateBusiness(
            dealer.Business ?? new Business(),
            updateDealerRequest.Business);

        //update the last completed on boarding step only if the current step is greater than previously completed step
        DealerStepEnum currentDealerStep =
            (DealerStepEnum) Enum.Parse(typeof(DealerStepEnum), updateDealerRequest.OnboardingStep.ToString());
        //once the dealer is onboarded the current dealer step will never be greater than last completed onboarding step and sign up mail will be sent just once
        if (currentDealerStep > dealer.LastCompletedOnboardingStep)
        {
            dealer.LastCompletedOnboardingStep = currentDealerStep;
            //mark the on boarding as complete only after the last step is complete
            dealer.Account.IsOnboardingComplete =
                dealer.LastCompletedOnboardingStep == DealerStepEnum.PublicCompanyProfile;
            if (dealer.Account.IsOnboardingComplete)
            {
                await _notificationService.NotifyDstOnDealerSignUp(dealer);
                await _paymentServiceClient.CreateDealerBankAccount(dealer.Account.Email, userType);
            }
        }

        dealer.Certifications = updateDealerRequest.Certifications;
        if (dealer.Account.IsOnboardingComplete)
        {
            await ClearQualifiedDealersCache();
        }

        return await _dealerRepository.UpdateDealer(dealer);
    }

    public async Task<DealerResponse> GetDealerWithBusinessLogo()
    {
        Dealer dealer = await GetDealer();
        DealerResponse dealerResponse = _mapper.Map<DealerResponse>(dealer);
        //if the business logo is previously uploaded
        int? logoBlobId = dealer.Business?.LogoBlobId;
        if (logoBlobId != null)
        {
            BlobResponse blobResponse = await _catalogServiceClient.GetBlobUrl((int) logoBlobId);
            if (dealerResponse.Business != null)
            {
                dealerResponse.Business.LogoUrl = blobResponse.BlobUrl;
            }
        }

        return dealerResponse;
    }

    public async Task<List<DealerResponse>> GetDealerByBusinessLocationIds(List<int> businessLocationIds,
        bool? isOnboardingComplete = null)
    {
        List<Dealer> dealers =
            await _dealerRepository.GetDealerByBusinessLocationIds(businessLocationIds, isOnboardingComplete);
        return _mapper.Map<List<DealerResponse>>(dealers);
    }

    public async Task<List<DealerResponse>> GetDealersByMatchCriteria(string zipcode, List<string> jobCategoryCodes,
        bool isOnboardingComplete)
    {
        // validate state and zip-code mapping
        StateByZipCodeResponse stateResponse = await _catalogServiceClient.GetStateAndCountyByZipCode(zipcode);
        if (stateResponse == null)
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidZipCode, StaticValues.ErrorInvalidZipCode);
        }

        List<BusinessLocationServiceableCounty> serviceableLocationsForCounty =
            await _businessLocationServiceableCountyRepository.FindByCounty(stateResponse.County.Name);

        List<int> businessLocationIds = serviceableLocationsForCounty
            .Select(x => x.BusinessLocation.Id)
            .ToList();

        List<DealerResponse> dealersWithMatchingZipCode =
            await GetDealerByBusinessLocationIds(businessLocationIds, isOnboardingComplete);

        if (jobCategoryCodes.IsNullOrEmpty())
        {
            return dealersWithMatchingZipCode;
        }

        List<DealerResponse> dealersWithFulfilledMatchCriteria = dealersWithMatchingZipCode.Where(dealer =>
        {
            List<string> categoryCodes = dealer.Business!.Categories
                .Select(x => x.Code)
                .ToList();
            // Check if passed job category codes are a subset of the categories that the business supports
            return jobCategoryCodes.All(categoryCodes.Contains);
        }).ToList();
        return dealersWithFulfilledMatchCriteria;
    }

    public async Task<List<DealerProfileResponse>> GetDealersBySearchStringAndState(string businessSearchString,
        string state)
    {
        List<Dealer> dealers = await _dealerRepository.GetDealersBySearchStringAndState(businessSearchString, state);
        return _mapper.Map<List<DealerProfileResponse>>(dealers);
    }

    public async Task<DealerLocationProfileResponse> GetDealerLocationProfile(int businessLocationId)
    {
        (Dealer? dealer, BusinessLocation businessLocation) =
            await GetDealerAndBusinessLocationById(businessLocationId);
        DealerLocationProfileResponse dealerLocationProfileResponse =
            await MapDealerAndBusinessLocationToDealerLocationResponse(dealer, businessLocation);
        return dealerLocationProfileResponse;
    }

    public async Task<(Dealer, BusinessLocation)> GetDealerAndBusinessLocationById(int businessLocationId)
    {
        List<Dealer> dealers =
            await _dealerRepository.GetDealerByBusinessLocationIds(new List<int> {businessLocationId});
        Dealer? dealer = dealers.FirstOrDefault();
        BusinessLocation? businessLocation = dealer?.Business?.Locations.FirstOrDefault();
        if (businessLocation == null)
        {
            throw new BusinessRuleViolationException(StaticValues.BusinessLocationNotFound,
                StaticValues.ErrorBusinessLocationNotFound(businessLocationId));
        }

        return (dealer!, businessLocation);
    }

    public async Task<DealerLocationProfileResponse> MapDealerAndBusinessLocationToDealerLocationResponse(
        Dealer dealer,
        BusinessLocation businessLocation
    )
    {
        DealerLocationProfileResponse dealerResponse = _mapper.Map<DealerLocationProfileResponse>(dealer);
        int? logoBlobId = dealer.Business?.LogoBlobId;
        if (logoBlobId != null)
        {
            BlobResponse blobResponse = await _catalogServiceClient.GetBlobUrl((int) logoBlobId);
            dealerResponse.Business.LogoUrl = blobResponse.BlobUrl;
        }

        dealerResponse.Business.Location = _mapper.Map<BusinessLocationResponse>(businessLocation);

        return dealerResponse;
    }

    public async Task<DealerTermsAndConditionsResponse> UpdateDealerTermsAndConditions(
        DealerTermsAndConditionsRequest dealerTermsAndConditionsRequest)
    {
        if (dealerTermsAndConditionsRequest.TermsAndConditionsAccepted == false)
        {
            throw new BusinessRuleViolationException(StaticValues.TermsAndConditionsNotAccepted,
                StaticValues.ErrorTermsAndConditionsNotAccepted);
        }

        Dealer dealer = await GetDealer();
        dealer = _mapper.Map(dealerTermsAndConditionsRequest, dealer);

        return _mapper.Map<DealerTermsAndConditionsResponse>(await _dealerRepository.UpdateDealer(dealer));
    }

    public async Task<Dealer> GetDealer()
    {
        int accountId = await _accountService.GetAccountId();
        Dealer? dealer = await _dealerRepository.GetDealerByAccountId(accountId);
        if (dealer == null)
        {
            throw new NotFoundException(StaticValues.DealerNotFound, StaticValues.ErrorDealerNotFound);
        }

        return dealer;
    }

    private async Task ClearQualifiedDealersCache()
    {
        RedisKey[] qualifiedDealersCacheKeys =
            _redisServer.Keys(pattern: StaticValues.QualifiedDealersCachePattern).ToArray();
        IEnumerable<Task> cacheClearTasks = qualifiedDealersCacheKeys.Select(
            qualifiedDealersCacheKey => _cache.RemoveAsync(qualifiedDealersCacheKey)
        );
        await Task.WhenAll(cacheClearTasks);
    }
}
