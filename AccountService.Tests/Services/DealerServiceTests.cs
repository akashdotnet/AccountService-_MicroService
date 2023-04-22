using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Config;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Dto.SalesForce;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using PodCommonsLibrary.Core.Exceptions;
using StackExchange.Redis;
using Xunit;

namespace AccountService.API.Tests.Services;

public class DealerServiceTests
{
    private readonly Mock<IAccountService> _accountServiceMock;

    private readonly Mock<IBusinessLocationServiceableCountyRepository>
        _businessLocationServiceableCountyRepositoryMock;

    private readonly Mock<IBusinessService> _businessServiceMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ICatalogServiceClient> _catalogServiceClientMock;
    private readonly Mock<IDealerRepository> _dealerRepositoryMock;

    private readonly DealerService _dealerService;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IPaymentServiceClient> _paymentServiceClientMock;
    private readonly Mock<IServer> _redisServerMock;

    public DealerServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        MapperConfiguration configuration = new(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(configuration);

        _accountServiceMock = new Mock<IAccountService>();
        _businessServiceMock = new Mock<IBusinessService>();
        _dealerRepositoryMock = new Mock<IDealerRepository>();
        _businessLocationServiceableCountyRepositoryMock = new Mock<IBusinessLocationServiceableCountyRepository>();
        _catalogServiceClientMock = new Mock<ICatalogServiceClient>();
        _accountServiceMock = new Mock<IAccountService>();
        _redisServerMock = new Mock<IServer>();
        _cacheMock = new Mock<IDistributedCache>();
        _paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        _notificationServiceMock = new Mock<INotificationService>();

        _dealerService = new DealerService(
            _accountServiceMock.Object,
            _businessServiceMock.Object,
            _dealerRepositoryMock.Object,
            _businessLocationServiceableCountyRepositoryMock.Object,
            _catalogServiceClientMock.Object,
            _mapper,
            _cacheMock.Object,
            _redisServerMock.Object,
            _paymentServiceClientMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact(DisplayName = "DealerService: CreateDealer - Should successfully create a dealer.")]
    public async Task CreateDealer_Success()
    {
        // arrange
        UserInformationResponse request = _fixture.Create<UserInformationResponse>();

        Dealer savedDealerMock = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.AboutBusiness)
            .Create();
        _dealerRepositoryMock.Setup(dealerRepository => dealerRepository
                .CreateDealer(It.IsAny<Dealer>()))
            .ReturnsAsync(savedDealerMock);

        // act
        DealerResponse result = await _dealerService.CreateDealer(request);

        // assert
        Assert.Equal(result.LastCompletedOnboardingStep, savedDealerMock.LastCompletedOnboardingStep);
    }

    [Fact(DisplayName =
        "DealerService: UpdateDealer - Should throw BusinessRuleViolationException with type RequiredBusinessName when business name field is not provided while creating business.")]
    public async Task UpdateDealer_RequiredBusinessName()
    {
        const string userType = "1";
        Dealer dealerMock = _fixture.Build<Dealer>()
            .Without(x => x.Business)
            .Create();
        UpdateDealerRequest updateDealerRequestMock = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.Business, _fixture.Build<BusinessRequest>()
                .Without(x => x.Name)
                .Create()
            )
            .Create();

        _dealerRepositoryMock.Setup(dealerRepository => dealerRepository
                .CreateDealer(It.IsAny<Dealer>()))
            .ReturnsAsync(dealerMock);

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);

        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _dealerService.UpdateDealer(updateDealerRequestMock, userType));

        Assert.Equal(StaticValues.RequiredBusinessName, businessRuleViolationException.ErrorResponseType);
        Assert.Equal("Business name field is required to create an business",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName = "DealerService: UpdateDealer - Should update the dealer for AboutBusiness step.")]
    public async Task UpdateDealer_AboutBusiness_Success()
    {
        const string userType = "1";
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.SignUpComplete)
            .Create();
        UpdateDealerRequest updateDealerRequestMock = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.AboutBusiness)
            .Create();

        Business businessMock = _fixture.Create<Business>();

        _businessServiceMock.Setup(businessService => businessService
                .CreateOrUpdateBusiness(It.IsAny<Business>(), updateDealerRequestMock.Business))
            .ReturnsAsync(businessMock);

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);
        _dealerRepositoryMock.Setup(m => m.UpdateDealer(dealerMock)).ReturnsAsync(dealerMock);

        Dealer result = await _dealerService.UpdateDealer(updateDealerRequestMock, userType);

        Assert.Equal(result, dealerMock);
        Assert.Equal(DealerStepEnum.AboutBusiness, dealerMock.LastCompletedOnboardingStep);
        Assert.False(dealerMock.Account.IsOnboardingComplete);

        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
        _businessServiceMock.VerifyAll();
        _notificationServiceMock.Verify(x => x.NotifyDstOnDealerSignUp(dealerMock), Times.Never);
    }

    [Fact(DisplayName =
        "DealerService: UpdateDealer - Should be able to create bank account for PublicCompanyProfile step.")]
    public async Task UpdateDealer_CreateBankAccount_Success()
    {
        //arrange
        const string userType = "1";
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.SignUpComplete)
            .Create();
        UpdateDealerRequest updateDealerRequestMock = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .Create();

        Business businessMock = _fixture.Create<Business>();

        _businessServiceMock.Setup(businessService => businessService
                .CreateOrUpdateBusiness(It.IsAny<Business>(), updateDealerRequestMock.Business))
            .ReturnsAsync(businessMock);

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);
        _dealerRepositoryMock.Setup(m => m.UpdateDealer(dealerMock)).ReturnsAsync(dealerMock);

        //act
        await _dealerService.UpdateDealer(updateDealerRequestMock, userType);

        //assert
        _paymentServiceClientMock.Verify(x => x.CreateDealerBankAccount(It.IsAny<string>(), userType), Times.Once);
    }

    [Fact(DisplayName =
        "DealerService: UpdateDealer - Should update the dealer for PublicCompanyProfile step, complete the dealer on-boarding and clear all the qualified dealers cache.")]
    public async Task UpdateDealer_PublicCompanyProfile_Success()
    {
        const string userType = "1";
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.AboutBusiness)
            .Create();
        UpdateDealerRequest updateDealerRequestMock = _fixture.Build<UpdateDealerRequest>()
            .With(x => x.OnboardingStep, DealerOnboardingStepEnum.PublicCompanyProfile)
            .Create();

        Business businessMock = _fixture.Create<Business>();

        _businessServiceMock.Setup(businessService => businessService
                .CreateOrUpdateBusiness(It.IsAny<Business>(), updateDealerRequestMock.Business))
            .ReturnsAsync(businessMock);

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);
        _dealerRepositoryMock.Setup(m => m.UpdateDealer(dealerMock)).ReturnsAsync(dealerMock);
        _redisServerMock.Setup(r => r.Keys(-1, "QUALIFIED_DEALERS*", 250, 0, 0, CommandFlags.None))
            .Returns(new List<RedisKey> {"QUALIFIED_DEALERS_#Ripple221"});
        _cacheMock.Setup(c => c.RemoveAsync("QUALIFIED_DEALERS_#Ripple221", default));
        Dealer result = await _dealerService.UpdateDealer(updateDealerRequestMock, userType);

        Assert.Equal(result, dealerMock);
        Assert.Equal(DealerStepEnum.PublicCompanyProfile, dealerMock.LastCompletedOnboardingStep);
        Assert.True(dealerMock.Account.IsOnboardingComplete);

        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
        _businessServiceMock.VerifyAll();
        _redisServerMock.Verify(r => r.Keys(-1, "QUALIFIED_DEALERS*", 250, 0, 0, CommandFlags.None), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("QUALIFIED_DEALERS_#Ripple221", default), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyDstOnDealerSignUp(dealerMock), Times.Once);
    }

    [Fact(DisplayName = "DealerService: GetDealer - Should be able get the dealer.")]
    public async Task GetDealer_Success()
    {
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.AboutBusiness)
            .Create();

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);

        Dealer result = await _dealerService.GetDealer();

        Assert.Equal(result, dealerMock);
        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName = "DealerService: GetDealer - Should throw NotFoundException when dealer could not be fetched.")]
    public async Task GetDealer_NotFound()
    {
        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync((Dealer?) null);

        NotFoundException notFoundException =
            await Assert.ThrowsAsync<NotFoundException>(async () => await _dealerService.GetDealer());

        Assert.Equal(StaticValues.DealerNotFound, notFoundException.ErrorResponseType);
        Assert.Equal("Unable to find dealer for the logged in user!", notFoundException.Message);

        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName =
        "DealerService: GetDealerWithBusinessLogo - Should get the dealer details with business logo url.")]
    public async Task GetDealerWithBusinessLogo_Success()
    {
        Business businessMock = _fixture.Build<Business>()
            .With(x => x.LogoBlobId, 1)
            .Create();
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.AboutBusiness)
            .With(x => x.Business, businessMock)
            .Create();
        BlobResponse blobResponseMock = _fixture.Build<BlobResponse>()
            .With(x => x.BlobUrl, "https://bloburl")
            .Create();

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);
        _catalogServiceClientMock.Setup(m => m.GetBlobUrl((int) dealerMock.Business.LogoBlobId))
            .ReturnsAsync(blobResponseMock);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);

        DealerResponse result = await _dealerService.GetDealerWithBusinessLogo();

        Assert.Equal(DealerStepEnum.AboutBusiness, result.LastCompletedOnboardingStep);
        Assert.Equal(blobResponseMock.BlobUrl, result.Business?.LogoUrl);

        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
        _catalogServiceClientMock.VerifyAll();
    }

    [Fact(DisplayName =
        "DealerService: GetDealerWithBusinessLogo - Should get the dealer details without business logo url if the logo is not previously updated.")]
    public async Task GetDealerWithBusinessLogo_Without_Logo_Id_Success()
    {
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.Business, _fixture.Build<Business>()
                .With(x => x.Name, "mock")
                .Without(x => x.LogoBlobId)
                .Create())
            .Create();

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);

        DealerResponse result = await _dealerService.GetDealerWithBusinessLogo();

        Assert.Equal("mock", result.Business.Name);

        _catalogServiceClientMock.Verify(catalogServiceClient => catalogServiceClient.GetBlobUrl(It.IsAny<int>()),
            Times.Never());
        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName =
        "DealerService: GetDealerWithBusinessLogo - Should get the dealer details without business logo if dealer is not mapped to business.")]
    public async Task GetDealerWithBusinessLogo_Without_Business_Success()
    {
        Dealer dealerMock = _fixture.Build<Dealer>()
            .Without(x => x.Business)
            .Create();

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);

        DealerResponse result = await _dealerService.GetDealerWithBusinessLogo();

        Assert.Null(result.Business);

        _catalogServiceClientMock.Verify(catalogServiceClient => catalogServiceClient.GetBlobUrl(It.IsAny<int>()),
            Times.Never());
        _accountServiceMock.VerifyAll();
        _dealerRepositoryMock.VerifyAll();
    }

    [Fact(DisplayName = "DealerService: GetDealerByBusinessLocationIds - Should filter and return list of dealers.")]
    public async Task GetDealerByBusinessLocationIds_Success()
    {
        // arrange
        List<int> businessLocationIds = new() {1, 2, 3};

        Dealer savedDealerMock1 = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.AboutBusiness).Create();
        Dealer savedDealerMock2 = _fixture.Build<Dealer>()
            .With(x => x.LastCompletedOnboardingStep, DealerStepEnum.PublicCompanyProfile).Create();

        List<Dealer> dealersMock = new()
        {
            savedDealerMock1,
            savedDealerMock2
        };
        _dealerRepositoryMock.Setup(dealerRepository => dealerRepository
                .GetDealerByBusinessLocationIds(It.IsAny<List<int>>(), null))
            .ReturnsAsync(dealersMock);
        // act
        List<DealerResponse> result = await _dealerService.GetDealerByBusinessLocationIds(businessLocationIds);

        // assert
        Assert.Equal(2, result.Count);
        Assert.Equal(DealerStepEnum.AboutBusiness, result[0].LastCompletedOnboardingStep);
        Assert.Equal(DealerStepEnum.PublicCompanyProfile, result[1].LastCompletedOnboardingStep);
    }

    [Fact(DisplayName =
        "DealerService: GetDealersByMatchCriteria - Should filter and return list of dealers that satisfy the match criteria.")]
    public async Task GetDealersByMatchCriteria_Success()
    {
        // arrange
        const string zipcode = "56010";
        List<string> jobCategoryCodes = new()
        {
            "pump_repair"
        };

        StateByZipCodeResponse stateByZipCodeResponseMock = _fixture.Create<StateByZipCodeResponse>();

        _catalogServiceClientMock.Setup(catalogServiceClient => catalogServiceClient
                .GetStateAndCountyByZipCode(It.IsAny<string>()))
            .ReturnsAsync(stateByZipCodeResponseMock);

        List<BusinessLocationServiceableCounty> serviceableLocationsForCountyMock = new()
        {
            _fixture.Build<BusinessLocationServiceableCounty>()
                .With(x => x.BusinessLocation, _fixture.Build<BusinessLocation>()
                    .With(x => x.Id, 1).Create())
                .Create(),
            _fixture.Build<BusinessLocationServiceableCounty>()
                .With(x => x.BusinessLocation, _fixture.Build<BusinessLocation>()
                    .With(x => x.Id, 2).Create())
                .Create()
        };

        _businessLocationServiceableCountyRepositoryMock.Setup(businessLocationServiceableCountyRepository =>
                businessLocationServiceableCountyRepository
                    .FindByCounty(It.IsAny<string>()))
            .ReturnsAsync(serviceableLocationsForCountyMock);

        List<BusinessJobCategory> businessJobCategoriesMock = new()
        {
            _fixture.Build<BusinessJobCategory>()
                .With(category => category.Code, "chemical_balancing")
                .Create(),
            _fixture.Build<BusinessJobCategory>()
                .With(category => category.Code, "pump_repair")
                .Create()
        };

        Business businessMock = _fixture.Build<Business>()
            .With(business => business.Locations, new List<BusinessLocation>
            {
                _fixture.Build<BusinessLocation>()
                    .With(x => x.Id, 1)
                    .Create(),
                _fixture.Build<BusinessLocation>()
                    .With(x => x.Id, 3)
                    .Create()
            })
            .With(business => business.Categories, businessJobCategoriesMock)
            .Create();

        List<Dealer> dealersMock = new()
        {
            _fixture.Build<Dealer>()
                .With(dealer => dealer.Business, businessMock)
                .Create(),
            _fixture.Create<Dealer>()
        };

        _dealerRepositoryMock.Setup(dealerRepository => dealerRepository
                .GetDealerByBusinessLocationIds(It.IsAny<List<int>>(), true))
            .ReturnsAsync(dealersMock);
        // act
        List<DealerResponse> result = await _dealerService.GetDealersByMatchCriteria(zipcode, jobCategoryCodes, true);
        List<string> codes = result[0].Business!.Categories.Select(jobCategory => jobCategory.Code).ToList();
        List<DealerResponse> resultWithNoCategories =
            await _dealerService.GetDealersByMatchCriteria(zipcode, null, true);

        // assert
        Assert.Single(result);
        Assert.Equal(2, resultWithNoCategories.Count);
        Assert.Contains(jobCategoryCodes[0], codes);
    }

    [Fact(DisplayName =
        "DealerService: GetDealersByMatchCriteria - Should throw BusinessRuleViolationException when no state and county belong to the passed zipcode.")]
    public async Task GetDealersByMatchCriteria_Success_BusinessNullCase()
    {
        // arrange
        const string zipcode = "56010";
        List<string> jobCategoryCodes = new()
        {
            "pump_repair"
        };
        _catalogServiceClientMock.Setup(catalogServiceClient => catalogServiceClient
                .GetStateAndCountyByZipCode(It.IsAny<string>()))!
            .ReturnsAsync((StateByZipCodeResponse) null!);
        // act
        BusinessRuleViolationException businessRuleViolationException =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await _dealerService.GetDealersByMatchCriteria(zipcode, jobCategoryCodes, true));

        Assert.Equal(StaticValues.InvalidZipCode, businessRuleViolationException.ErrorResponseType);
        Assert.Equal("Invalid Zip Code!",
            businessRuleViolationException.Message);
    }

    [Fact(DisplayName =
        "DealerService: GetDealerLocationProfile - Should successfully get the business location and the related dealer based on business location id.")]
    public async Task GetDealerLocationProfile_Success()
    {
        const int businessLocationIdMock = 1;
        BusinessLocation businessLocationMock = _fixture.Build<BusinessLocation>()
            .With(x => x.Id, businessLocationIdMock)
            .Create();
        Business businessMock = _fixture.Build<Business>()
            .With(x => x.Locations, new List<BusinessLocation> {businessLocationMock})
            .Without(x => x.LogoBlobId)
            .Create();
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.Account, new Account {Email = "test@mail.com"})
            .With(x => x.Business, businessMock)
            .Create();

        // arrange
        _dealerRepositoryMock.Setup(x => x.GetDealerByBusinessLocationIds(new List<int> {businessLocationIdMock}, null))
            .ReturnsAsync(new List<Dealer> {dealerMock});

        //act
        DealerLocationProfileResponse result =
            await _dealerService.GetDealerLocationProfile(businessLocationIdMock);

        // assert
        Assert.IsType<DealerLocationProfileResponse>(result);
        Assert.NotNull(result);
        Assert.Equal(dealerMock.Account.Email, result.Email);
        Assert.Equal(businessLocationMock.Id, result.Business.Location.Id);
        Assert.Equal(businessMock.Name, result.Business.Name);
        Assert.Equal(businessMock.PoolCount, result.Business.PoolCount);
        Assert.Equal(businessMock.WebsiteUrl, result.Business.WebsiteUrl);
        _dealerRepositoryMock.Verify(
            x => x.GetDealerByBusinessLocationIds(new List<int> {businessLocationIdMock}, null),
            Times.Once);
    }

    [Fact(DisplayName =
        "DealerService: GetDealerLocationProfile - Should successfully get the business location and the business logo url based on business location id.")]
    public async Task GetDealerLocationProfile_WithBusinessLogoUrl_Success()
    {
        const int businessLocationIdMock = 1;
        const int logoBlobIdMock = 1;
        BusinessLocation businessLocationMock = _fixture.Build<BusinessLocation>()
            .With(x => x.Id, businessLocationIdMock)
            .Create();
        Business businessMock = _fixture.Build<Business>()
            .With(x => x.Locations, new List<BusinessLocation> {businessLocationMock})
            .With(x => x.LogoBlobId, logoBlobIdMock)
            .Create();
        BlobResponse blobResponseMock = _fixture.Build<BlobResponse>()
            .With(x => x.BlobId, logoBlobIdMock)
            .Create();
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.Account, new Account {Email = "test@mail.com"})
            .With(x => x.Business, businessMock)
            .Create();

        // arrange
        _dealerRepositoryMock.Setup(x => x.GetDealerByBusinessLocationIds(new List<int> {businessLocationIdMock}, null))
            .ReturnsAsync(new List<Dealer> {dealerMock});
        _catalogServiceClientMock.Setup(x => x.GetBlobUrl(logoBlobIdMock)).ReturnsAsync(blobResponseMock);
        //act
        DealerLocationProfileResponse result =
            await _dealerService.GetDealerLocationProfile(businessLocationIdMock);

        // assert
        Assert.IsType<DealerLocationProfileResponse>(result);
        Assert.NotNull(result);
        Assert.Equal(dealerMock.Account.Email, result.Email);
        Assert.Equal(businessLocationMock.Id, result.Business.Location.Id);
        Assert.Equal(businessMock.Name, result.Business.Name);
        Assert.Equal(businessMock.PoolCount, result.Business.PoolCount);
        Assert.Equal(businessMock.WebsiteUrl, result.Business.WebsiteUrl);
        Assert.Equal(blobResponseMock.BlobUrl, result.Business.LogoUrl);
        _dealerRepositoryMock.Verify(
            x => x.GetDealerByBusinessLocationIds(new List<int> {businessLocationIdMock}, null),
            Times.Once);
        _catalogServiceClientMock.Verify(x => x.GetBlobUrl(logoBlobIdMock), Times.Once);
    }

    [Fact(DisplayName =
        "DealerService: GetDealerProfilesBySearchString - Should throw business rule exception when business location could not be found for the given business location id.")]
    public async Task GetDealerLocationProfile_BusinessLocationNotFound_ThrowsBusinessRuleViolationException()
    {
        const int businessLocationIdMock = 1;
        _dealerRepositoryMock.Setup(x => x.GetDealerByBusinessLocationIds(new List<int> {businessLocationIdMock}, null))
            .ReturnsAsync(new List<Dealer>());

        BusinessRuleViolationException exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await _dealerService.GetDealerLocationProfile(businessLocationIdMock));
        // assert
        Assert.NotNull(exception);
        Assert.Equal(StaticValues.BusinessLocationNotFound, exception.ErrorResponseType);
        Assert.Equal("Unable to find the business location with id: 1", exception.Message);
    }

    [Fact(DisplayName =
        "DealerService: GetDealersBySearchStringAndState - Should successfully filter the dealers whose business name starts with same value as businessSearchString input.")]
    public async Task GetDealerProfilesByState_Success()
    {
        const string businessSearchStringMock = "test";
        const string stateMock = "Arkansas";
        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.Account, new Account {Email = "test@mail.com"})
            .With(x => x.Business, new Business {Name = "test-business-name"})
            .Create();

        // arrange
        _dealerRepositoryMock.Setup(x => x.GetDealersBySearchStringAndState(businessSearchStringMock, stateMock))
            .ReturnsAsync(new List<Dealer> {dealerMock});

        //act
        List<DealerProfileResponse> result =
            await _dealerService.GetDealersBySearchStringAndState(businessSearchStringMock, stateMock);

        // assert
        Assert.IsType<List<DealerProfileResponse>>(result);
        Assert.Single(result);
        Assert.Equal(result[0].Business.Name, dealerMock?.Business?.Name);
        Assert.Equal(result[0].Email, dealerMock?.Account.Email);
        _dealerRepositoryMock.Verify(x => x.GetDealersBySearchStringAndState(businessSearchStringMock, stateMock),
            Times.Once);
    }

    [Fact(DisplayName =
        "DealerService: UpdateDealerTermsAndConditions - Should successfully update the dealer's terms and conditions.")]
    public async Task UpdateDealerTermsAndConditions_Success()
    {
        //arrange
        DealerTermsAndConditionsRequest dealerTermsAndConditionsRequest = _fixture
            .Build<DealerTermsAndConditionsRequest>()
            .With(x => x.TermsAndConditionsAccepted, true)
            .Create();
        Dealer dealerMock = _fixture.Create<Dealer>();
        Dealer savedDealerMock = _fixture.Build<Dealer>()
            .With(x => x.TermsAndConditionsAccepted, true)
            .Create();

        _accountServiceMock.Setup(m => m.GetAccountId(null)).ReturnsAsync(1);
        _dealerRepositoryMock.Setup(m => m.GetDealerByAccountId(1)).ReturnsAsync(dealerMock);
        _dealerRepositoryMock.Setup(m => m.UpdateDealer(dealerMock)).ReturnsAsync(savedDealerMock);

        //act
        DealerTermsAndConditionsResponse result =
            await _dealerService.UpdateDealerTermsAndConditions(dealerTermsAndConditionsRequest);

        //assert
        Assert.IsType<DealerTermsAndConditionsResponse>(result);
        Assert.Equal(dealerTermsAndConditionsRequest.TermsAndConditionsAccepted, result.TermsAndConditionsAccepted);
    }

    [Fact(DisplayName =
        "DealerService: UpdateDealerTermsAndConditions - Should throw business rule exception when terms and conditions are not accepted.")]
    public async Task UpdateDealerTermsAndConditions_BusinessRuleViolationException()
    {
        //arrange
        DealerTermsAndConditionsRequest dealerTermsAndConditionsRequest = _fixture
            .Build<DealerTermsAndConditionsRequest>()
            .With(x => x.TermsAndConditionsAccepted, false)
            .Create();

        //act
        BusinessRuleViolationException exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await _dealerService.UpdateDealerTermsAndConditions(dealerTermsAndConditionsRequest));

        // assert
        Assert.NotNull(exception);
        Assert.Equal(StaticValues.TermsAndConditionsNotAccepted, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorTermsAndConditionsNotAccepted, exception.Message);
    }
}
