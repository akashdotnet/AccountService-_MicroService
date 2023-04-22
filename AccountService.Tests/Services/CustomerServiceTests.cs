using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Config;
using AccountService.API.Config.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.CustomerServiceClient;
using AccountService.API.Dto.NotificationServiceClient;
using AccountService.API.Dto.PaymentServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Enums;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICatalogServiceClient> _catalogServiceClientMock;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IAddressRepository> _mockAddressRepository;
    private readonly Mock<IAddressService> _mockAddressService;
    private readonly Mock<AppConfig> _mockAppConfig;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ICustomerDeliveryInstructionRepository> _mockCustomerDeliveryInstructionRepository;
    private readonly Mock<ICustomerFavouriteDealerMappingRepository> _mockCustomerFavouriteDealerMappingRepository;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<ICustomerServiceClient> _mockCustomerServiceClient;
    private readonly Mock<ICustomerSuggestedDealerRepository> _mockCustomerSuggestedDealerRepository;
    private readonly Mock<IDealerService> _mockDealerService;
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly Mock<IPaymentServiceClient> _mockPaymentServiceClient;
    private readonly Mock<IServiceBusConfig> _mockServiceBusConfig;
    private readonly Mock<UserManager<IdentityUser>> _userManager;

    public CustomerServiceTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockCustomerFavouriteDealerMappingRepository = new Mock<ICustomerFavouriteDealerMappingRepository>();
        _mockCustomerDeliveryInstructionRepository = new Mock<ICustomerDeliveryInstructionRepository>();
        _mockCustomerSuggestedDealerRepository = new Mock<ICustomerSuggestedDealerRepository>();
        _mockAccountService = new Mock<IAccountService>();
        _mockDealerService = new Mock<IDealerService>();
        _mockAddressRepository = new Mock<IAddressRepository>();
        _mockAddressService = new Mock<IAddressService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockPaymentServiceClient = new Mock<IPaymentServiceClient>();
        _mockCustomerServiceClient = new Mock<ICustomerServiceClient>();
        _catalogServiceClientMock = new Mock<ICatalogServiceClient>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _fixture = new Fixture();
        _userManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null);
        //add in the ordered customizations (put derived ones before base ones)
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mapper = new MapperConfiguration(c =>
            c.AddProfile<MappingProfile>()).CreateMapper();

        _mockDistributedCache = new Mock<IDistributedCache>();
        Mock<IConfiguration> mockConfig = new();
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "OtpLength")]).Returns("6");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "OtpCoolDownTimeInSeconds")]).Returns("60");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "OtpValidityInDays")]).Returns("10");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "ServiceBusConnectionString")])
            .Returns("sb-connection-string");
        mockConfig.SetupGet(x => x[It.Is<string>(s => s == "ServiceBusEmailQueue")]).Returns("email-q");
        _mockAppConfig = new Mock<AppConfig>(mockConfig.Object);
        _mockServiceBusConfig = new Mock<IServiceBusConfig>();
        Mock<DistributedCacheEntryOptions> mockDistributedCacheEntryOptions = new();

        _mockDistributedCache.Setup(x =>
                x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), mockDistributedCacheEntryOptions.Object, default))
            .Returns(Task.FromResult(true));
        _mockDistributedCache.Setup(x =>
                x.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.FromResult(true));
    }

    [Fact(DisplayName =
        "CustomerService: UpdateCustomerNotificationPreferences - Should be able to update customer notification preferences.")]
    public async Task UpdateCustomerNotificationPreferences_Success()
    {
        // arrange
        Customer customer = _fixture.Create<Customer>();
        NotificationPreferenceRequest notificationPreferenceRequest = _fixture.Create<NotificationPreferenceRequest>();
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.UpdateCustomer(It.IsAny<Customer>()));

        // act
        await CustomerServiceObject()
            .UpdateCustomerNotificationPreferences(notificationPreferenceRequest);

        // assert
        _mockAccountService.Verify(x => x.GetAccountId(It.IsAny<string>()), Times.Once);
        _mockCustomerRepository.Verify(x => x.UpdateCustomer(It.IsAny<Customer>()), Times.Once);
    }


    [Fact(DisplayName =
        "CustomerService: GetNotificationPreferences - Should be able to get customer notification preferences.")]
    public async Task GetNotificationPreferences_Success()
    {
        // arrange
        Customer customer = _fixture.Create<Customer>();
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);

        // act
        NotificationPreferenceResponse result = await CustomerServiceObject()
            .GetNotificationPreferences();

        // assert
        _mockAccountService.Verify(x => x.GetAccountId(It.IsAny<string>()), Times.Once);
        Assert.IsType<NotificationPreferenceResponse>(result);
    }

    [Fact(DisplayName = "CustomerService: CreateCustomer - Should be able to create customer.")]
    public async Task CreateCustomer_Success()
    {
        // arrange
        const string firstName = "FirstName";
        const string lastName = "LastName";
        CustomerRegistrationRequest customerRegistrationRequest = _fixture.Build<CustomerRegistrationRequest>()
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        Session customerSession = _fixture.Create<Session>();
        _mockCustomerRepository.Setup(x => x.GetCustomerByEmail(customerRegistrationRequest.Email))
            .ReturnsAsync((Customer?) null);

        _mockCustomerRepository.Setup(x => x.CreateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);

        // act
        CustomerResponse result =
            await CustomerServiceObject().CreateCustomer(customerRegistrationRequest, customerSession);

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.FirstName, customer.Account.FirstName);
        Assert.Equal(result.LastName, customer.Account.LastName);
        Assert.Equal(CustomerStepEnum.SignUpComplete, customer.LastCompletedOnboardingStep);
        _mockCustomerRepository.Verify(x => x.GetCustomerByEmail(customerRegistrationRequest.Email), Times.Once);

        _mockCustomerRepository.Verify(x => x.CreateCustomer(It.IsAny<Customer>()), Times.Once);
    }

    [Fact(DisplayName = "CustomerService: CreateCustomer - Should be able to restore the deleted customer account.")]
    public async Task CreateCustomer_RestoreAccount_Success()
    {
        // arrange
        const string firstName = "FirstName";
        const string lastName = "LastName";
        CustomerRegistrationRequest customerRegistrationRequest = _fixture.Build<CustomerRegistrationRequest>()
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        Customer deletedCustomer = _fixture.Build<Customer>()
            .With(x => x.IsDeleted, true)
            .Create();
        Session customerSession = _fixture.Create<Session>();
        _mockCustomerRepository.Setup(x => x.GetCustomerByEmail(customerRegistrationRequest.Email))
            .ReturnsAsync(deletedCustomer);

        _mockCustomerRepository.Setup(x => x.UpdateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);

        // act
        CustomerResponse result =
            await CustomerServiceObject().CreateCustomer(customerRegistrationRequest, customerSession);

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.FirstName, customer.Account.FirstName);
        Assert.Equal(result.LastName, customer.Account.LastName);
        Assert.Equal(CustomerStepEnum.SignUpComplete, customer.LastCompletedOnboardingStep);
        _mockCustomerRepository.Verify(x => x.GetCustomerByEmail(customerRegistrationRequest.Email), Times.Once);

        _mockCustomerRepository.Verify(x => x.UpdateCustomer(It.IsAny<Customer>()), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerService: CreateCustomer - Should throw business rule violation exception if a non-deleted customer account exists with same email address.")]
    public async Task CreateCustomer_ExistingNonDeletedCustomerAccount_ThrowsBusinessRuleViolationException()
    {
        // arrange
        const string firstName = "FirstName";
        const string lastName = "LastName";
        CustomerRegistrationRequest customerRegistrationRequest = _fixture.Build<CustomerRegistrationRequest>()
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.IsDeleted, false)
            .Create();
        Session customerSession = _fixture.Create<Session>();
        _mockCustomerRepository.Setup(x => x.GetCustomerByEmail(customerRegistrationRequest.Email))
            .ReturnsAsync(customer);
        // act and assert
        BusinessRuleViolationException exception =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await CustomerServiceObject().CreateCustomer(customerRegistrationRequest, customerSession));
        Assert.NotNull(exception);
        Assert.Equal(StaticValues.UserWithSameEmailExists, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorUserRegister, exception.Message);
    }

    [Fact(DisplayName =
        "CustomerService: CreateCustomer - Should fail while creating customer due to repository exception.")]
    public async Task CreateCustomer_Failure()
    {
        // arrange
        const string firstName = "FirstName";
        const string lastName = "LastName";
        CustomerRegistrationRequest customerRegistrationRequest = _fixture.Build<CustomerRegistrationRequest>()
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .Create();
        _mockCustomerRepository.Setup(x => x.CreateCustomer(It.IsAny<Customer>())).ThrowsAsync(new Exception());

        // act and assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await CustomerServiceObject().CreateCustomer(customerRegistrationRequest, _fixture.Create<Session>()));
    }

    [Fact(DisplayName =
        "CustomerService: UpdateCustomer - Should be able to update customer during signUp complete step.")]
    public async Task UpdateCustomer_SignUpComplete_Success()
    {
        // arrange
        const string phoneNumber = "1234567890";
        const string zipCode = "99150";
        const string validState = "California";
        const string countyName = "Jefferson";

        CustomerAddressRequest customerAddress = _fixture.Build<CustomerAddressRequest>()
            .With(x => x.City, countyName)
            .With(x => x.State, validState)
            .With(x => x.ZipCode, zipCode)
            .Create();

        UpdateCustomerRequest updateCustomerRequest = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.SignUpComplete)
            .With(x => x.PhoneNumber, phoneNumber)
            .With(x => x.Address, customerAddress)
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.PhoneNumber, phoneNumber)
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();

        Address address = _fixture.Build<Address>()
            .With(x => x.ZipCode, zipCode)
            .Create();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.CreateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);
        _mockAddressRepository.Setup(x => x.CreateOrUpdateAddress(It.IsAny<Address>())).ReturnsAsync(address);
        _mockCustomerRepository.Setup(x => x.UpdateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);
        // act
        CustomerResponse result = await CustomerServiceObject().UpdateCustomer(updateCustomerRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.PhoneNumber, account.PhoneNumber);
        Assert.Contains(result.Addresses, x => x.ZipCode == zipCode);
        Assert.False(result.IsOnboardingComplete);
    }

    [Fact(DisplayName =
        "CustomerService: UpdateCustomer - Should be able to update customer and create their Stripe account during pool details onboarding step.")]
    public async Task UpdateCustomer_PoolDetailsStep_Success()
    {
        // arrange
        const string sanitationMethod = "water_pumps";
        const string poolType = "lap_pool";
        const string poolSize = "medium";
        const string poolSeason = "open_closed_seasons";
        const string poolMaterial = "fibreglass";
        const string hotTubType = "none";
        const string phoneNumber = "1234567890";
        const string zipCode = "99150";
        const string validState = "California";
        const string countyName = "Jefferson";

        CustomerAddressRequest customerAddress = _fixture.Build<CustomerAddressRequest>()
            .With(x => x.City, countyName)
            .With(x => x.State, validState)
            .With(x => x.ZipCode, zipCode)
            .Create();
        UpdateCustomerRequest updateCustomerRequest = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.PoolDetails)
            .With(x => x.SanitationMethodCode, sanitationMethod)
            .With(x => x.PoolSizeCode, poolSize)
            .With(x => x.PoolTypeCode, poolType)
            .With(x => x.PoolMaterialCode, poolMaterial)
            .With(x => x.HotTubTypeCode, hotTubType)
            .With(x => x.PoolSeasonCode, poolSeason)
            .With(x => x.Address, customerAddress)
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.PhoneNumber, phoneNumber)
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();
        Address address = _fixture.Build<Address>()
            .With(x => x.ZipCode, zipCode)
            .Create();
        CreateCustomerForPaymentResponse
            customerForPaymentResponse = _fixture.Build<CreateCustomerForPaymentResponse>()
                .With(x => x.StripeCustomerId, "stripe-customer-id")
                .Create();
        List<ReminderResponse> remindersResponse = _fixture.Create<List<ReminderResponse>>();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockPaymentServiceClient.Setup(x => x.CreateCustomer(It.IsAny<CreateCustomerForPaymentRequest>()))
            .ReturnsAsync(customerForPaymentResponse);
        _mockCustomerServiceClient.Setup(x => x.CreateCustomerReminders(It.IsAny<CreateCustomerReminderRequest>()))
            .ReturnsAsync(remindersResponse);
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.CreateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);
        _mockAddressRepository.Setup(x => x.CreateOrUpdateAddress(It.IsAny<Address>())).ReturnsAsync(address);
        _mockCustomerRepository.Setup(x => x.UpdateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);

        // act
        CustomerResponse result = await CustomerServiceObject().UpdateCustomer(updateCustomerRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.PhoneNumber, account.PhoneNumber);
        Assert.Equal(sanitationMethod, result.SanitationMethodCode);
        Assert.Equal(poolSize, result.PoolSizeCode);
        Assert.Equal(poolType, result.PoolTypeCode);
        Assert.Equal(poolMaterial, result.PoolMaterialCode);
        Assert.Equal(hotTubType, result.HotTubTypeCode);
        Assert.Equal(poolSeason, result.PoolSeasonCode);
        Assert.True(result.IsOnboardingComplete);
    }

    [Fact(DisplayName =
        "CustomerService: UpdateCustomer - Should be able to update customer during edit profile step.")]
    public async Task UpdateCustomer_EditProfile_Success()
    {
        // arrange
        const string firstName = "Stella";
        const string lastName = "Black";
        const string phoneNumber = "1234567890";

        UpdateCustomerRequest updateCustomerRequest = _fixture.Build<UpdateCustomerRequest>()
            .With(x => x.OnboardingStep, CustomerStepEnum.EditProfile)
            .With(x => x.PhoneNumber, phoneNumber)
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .Create();
        Account account = _fixture.Build<Account>()
            .With(x => x.PhoneNumber, phoneNumber)
            .With(x => x.FirstName, firstName)
            .With(x => x.LastName, lastName)
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();

        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.CreateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.UpdateCustomer(It.IsAny<Customer>())).ReturnsAsync(customer);

        // act
        CustomerResponse result = await CustomerServiceObject().UpdateCustomer(updateCustomerRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.PhoneNumber, account.PhoneNumber);
        Assert.Equal(result.FirstName, account.FirstName);
        Assert.Equal(result.LastName, account.LastName);
        Assert.True(result.IsOnboardingComplete);
    }


    [Fact(DisplayName = "CustomerService: UpdateCustomer - Unable to update customer as existing customer not found.")]
    public async Task UpdateCustomer_CustomerNotFound_Failure()
    {
        // arrange
        UpdateCustomerRequest updateCustomerRequest = _fixture.Create<UpdateCustomerRequest>();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync((Customer?) null);

        // act and assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await CustomerServiceObject().UpdateCustomer(updateCustomerRequest));
        Assert.Equal(StaticValues.CustomerNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorCustomerNotFound, exception.Message);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomer - Should be able to get customer details with profile picture url.")]
    public async Task GetCustomer_Success()
    {
        // arrange
        Customer customer = _fixture.Create<Customer>();
        BlobResponse blobResponse = _fixture.Build<BlobResponse>()
            .With(x => x.BlobUrl, "https://bloburl")
            .Create();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _catalogServiceClientMock.Setup(x => x.GetBlobUrl(It.IsAny<int>())).ReturnsAsync(blobResponse);

        // act
        CustomerResponse result = await CustomerServiceObject().GetCustomer();

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.FirstName, customer.Account.FirstName);
        Assert.Equal(result.LastName, customer.Account.LastName);
        Assert.Equal(result.Email, customer.Account.Email);
        Assert.Equal(result.PhoneNumber, customer.Account.PhoneNumber);
        Assert.Equal(result.IsOnboardingComplete, customer.Account.IsOnboardingComplete);
        Assert.Equal(blobResponse.BlobUrl, result.ProfilePhotoUrl);
        Assert.Equal(result.PasswordResetDate, customer.PasswordResetDate);
    }

    [Fact(DisplayName =
        "CustomerService: DeleteCustomerProfilePicture - Should throw business rule violation exception when customer profile picture is not found.")]
    public async Task DeleteCustomerProfilePicture_Failure()
    {
        // arrange
        const string email = "customer@customer.com";
        const int accountId = 1;
        Customer customer = _fixture.Build<Customer>().With(x => x.ProfilePhotoBlobId, (int?) null).Create();
        _mockAccountService.Setup(x => x.GetAccountId(email)).ReturnsAsync(accountId);
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(accountId)).ReturnsAsync(customer);

        // act
        BusinessRuleViolationException result = await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
            await CustomerServiceObject().DeleteCustomerProfilePicture(email));

        // assert
        Assert.Equal(StaticValues.ErrorProfilePictureNotFound, result.Message);
        Assert.Equal(StaticValues.ProfilePictureNotFound, result.ErrorResponseType);
    }

    [Fact(DisplayName =
        "CustomerService: DeleteCustomerProfilePicture - Should be able to delete customer profile picture.")]
    public async Task DeleteCustomerProfilePicture_Success()
    {
        // arrange
        const string email = "customer@customer.com";
        const int accountId = 1;
        Customer customer = _fixture.Build<Customer>().With(x => x.ProfilePhotoBlobId, 1).Create();
        Customer updatedCustomer = _fixture.Build<Customer>().With(x => x.ProfilePhotoBlobId, (int?) null).Create();
        _mockAccountService.Setup(x => x.GetAccountId(email)).ReturnsAsync(accountId);
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(accountId)).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.UpdateCustomer(customer)).ReturnsAsync(updatedCustomer);

        // act
        await CustomerServiceObject().DeleteCustomerProfilePicture(email);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomer - Should be able to get customer details without profile picture url if the picture is not previously updated.")]
    public async Task GetCustomer_Without_Profile_Picture_Id_Success()
    {
        // arrange
        Customer customer = _fixture
            .Build<Customer>()
            .Without(x => x.ProfilePhotoBlobId)
            .Create();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);

        // act
        CustomerResponse result = await CustomerServiceObject().GetCustomer();

        // assert
        Assert.NotNull(result);
        Assert.Equal(result.FirstName, customer.Account.FirstName);
        Assert.Equal(result.LastName, customer.Account.LastName);
        Assert.Equal(result.Email, customer.Account.Email);
        Assert.Equal(result.PhoneNumber, customer.Account.PhoneNumber);
        Assert.Equal(result.IsOnboardingComplete, customer.Account.IsOnboardingComplete);
        Assert.Equal(result.PasswordResetDate, customer.PasswordResetDate);
    }

    [Fact(DisplayName = "CustomerService: GetCustomer - Unable to get customer as existing customer not found.")]
    public async Task GetCustomer_CustomerNotFound_Failure()
    {
        // arrange
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync((Customer?) null);

        // act and assert
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(async () => await CustomerServiceObject().GetCustomer());
        Assert.Equal(StaticValues.CustomerNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorCustomerNotFound, exception.Message);
    }

    [Fact(DisplayName =
        "CustomerService: SaveFavouriteDealer - Should assign favourite dealer to the customer.")]
    public async Task SaveFavouriteDealer_CreateFavouriteDealer_Success()
    {
        // arrange
        const int businessLocationId = 1;

        BusinessLocation businessLocationMock = _fixture.Build<BusinessLocation>()
            .With(x => x.Id, businessLocationId)
            .Create();
        Account dealerAccountMock = _fixture.Build<Account>()
            .With(x => x.Email, "test@mail.com")
            .With(x => x.UserRole, UserRoleEnum.Dealer)
            .Create();

        Business businessMock = _fixture.Build<Business>()
            .With(x => x.Name, "test business name")
            .With(x => x.About, "test business about")
            .With(x => x.StartYear, 2001)
            .With(x => x.PoolCount, 1)
            .Without(x => x.LogoBlobId)
            .With(x => x.PhoneNumber, "888888888")
            .With(x => x.WebsiteUrl, "https://test.com")
            .With(x => x.Locations, new List<BusinessLocation> {businessLocationMock})
            .Create();

        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.Business, businessMock)
            .With(x => x.Account, dealerAccountMock)
            .Create();
        FavouriteDealerRequest favouriteDealerRequest = _fixture.Build<FavouriteDealerRequest>()
            .With(x => x.BusinessLocationId, businessLocationId)
            .Create();
        Account customerAccountMock = _fixture.Build<Account>()
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, 1)
            .With(x => x.Account, customerAccountMock)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();

        CustomerFavouriteDealerMapping existingCustomerFavouriteDealerMappingMock =
            _fixture.Build<CustomerFavouriteDealerMapping>()
                .With(x => x.Customer, customer)
                .With(x => x.BusinessLocationId, 6)
                .Create();

        CustomerFavouriteDealerMapping customerFavouriteDealerMappingMock =
            _fixture.Build<CustomerFavouriteDealerMapping>()
                .With(x => x.Customer, customer)
                .With(x => x.BusinessLocationId, businessLocationId)
                .Create();
        DealerLocationProfileResponse favouriteDealerResponse = _fixture.Create<DealerLocationProfileResponse>();

        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerFavouriteDealerMappingRepository.Setup(x => x.GetCustomerFavouriteDealerMapping(1))
            .ReturnsAsync(existingCustomerFavouriteDealerMappingMock);
        _mockDealerService.Setup(x => x.GetDealerAndBusinessLocationById(businessLocationId)).ReturnsAsync(
            (dealerMock, businessLocationMock));
        _mockDealerService.Setup(x =>
                x.MapDealerAndBusinessLocationToDealerLocationResponse(dealerMock, businessLocationMock))
            .ReturnsAsync(favouriteDealerResponse);
        _mockCustomerFavouriteDealerMappingRepository.Setup(x =>
                x.CreateOrUpdateCustomerFavouriteDealerMapping(It.IsAny<CustomerFavouriteDealerMapping>()))
            .ReturnsAsync(customerFavouriteDealerMappingMock);

        // act
        DealerLocationProfileResponse
            result = await CustomerServiceObject().SaveFavouriteDealer(favouriteDealerRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal(favouriteDealerResponse, result);
        _mockDealerService.Verify(x =>
            x.MapDealerAndBusinessLocationToDealerLocationResponse(dealerMock, businessLocationMock), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerService: SaveFavouriteDealer - Should update the favourite dealer of the customer.")]
    public async Task SaveFavouriteDealer_UpdateFavouriteDealer_Success()
    {
        // arrange
        const int businessLocationId = 1;

        BusinessLocation businessLocationMock = _fixture.Build<BusinessLocation>()
            .With(x => x.Id, businessLocationId)
            .Create();
        Account dealerAccountMock = _fixture.Build<Account>()
            .With(x => x.Email, "test@mail.com")
            .With(x => x.UserRole, UserRoleEnum.Dealer)
            .Create();

        Business businessMock = _fixture.Build<Business>()
            .With(x => x.Name, "test business name")
            .With(x => x.About, "test business about")
            .With(x => x.StartYear, 2001)
            .With(x => x.PoolCount, 1)
            .With(x => x.LogoBlobId, 1)
            .With(x => x.PhoneNumber, "888888888")
            .With(x => x.WebsiteUrl, "https://test.com")
            .With(x => x.Locations, new List<BusinessLocation> {businessLocationMock})
            .Create();

        Dealer dealerMock = _fixture.Build<Dealer>()
            .With(x => x.Business, businessMock)
            .With(x => x.Account, dealerAccountMock)
            .Create();
        FavouriteDealerRequest favouriteDealerRequest = _fixture.Build<FavouriteDealerRequest>()
            .With(x => x.BusinessLocationId, businessLocationId)
            .Create();
        Account customerAccountMock = _fixture.Build<Account>()
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, 1)
            .With(x => x.Account, customerAccountMock)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();

        CustomerFavouriteDealerMapping existingCustomerFavouriteDealerMappingMock =
            _fixture.Build<CustomerFavouriteDealerMapping>()
                .With(x => x.Customer, customer)
                .With(x => x.BusinessLocationId, 6)
                .Create();

        CustomerFavouriteDealerMapping customerFavouriteDealerMappingMock =
            _fixture.Build<CustomerFavouriteDealerMapping>()
                .With(x => x.Customer, customer)
                .With(x => x.BusinessLocationId, businessLocationId)
                .Create();
        DealerLocationProfileResponse favouriteDealerResponse = _fixture.Create<DealerLocationProfileResponse>();

        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerFavouriteDealerMappingRepository.Setup(x => x.GetCustomerFavouriteDealerMapping(1))
            .ReturnsAsync(existingCustomerFavouriteDealerMappingMock);
        _mockDealerService.Setup(x => x.GetDealerAndBusinessLocationById(businessLocationId)).ReturnsAsync(
            (dealerMock, businessLocationMock));
        _mockDealerService.Setup(x =>
                x.MapDealerAndBusinessLocationToDealerLocationResponse(dealerMock, businessLocationMock))
            .ReturnsAsync(favouriteDealerResponse);

        _mockCustomerFavouriteDealerMappingRepository.Setup(x =>
                x.CreateOrUpdateCustomerFavouriteDealerMapping(It.IsAny<CustomerFavouriteDealerMapping>()))
            .ReturnsAsync(customerFavouriteDealerMappingMock);

        // act
        DealerLocationProfileResponse
            result = await CustomerServiceObject().SaveFavouriteDealer(favouriteDealerRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal(favouriteDealerResponse, result);
        _mockDealerService.Verify(x =>
            x.MapDealerAndBusinessLocationToDealerLocationResponse(dealerMock, businessLocationMock), Times.Once);
    }

    [Fact(DisplayName =
        " CustomerService: SaveFavouriteDealer - Should throw not found exception if no business location exists with given id.")]
    public async Task SaveFavouriteDealer_CustomerNotFound()
    {
        // arrange
        const int businessLocationId = 1;
        FavouriteDealerRequest favouriteDealerRequest = _fixture.Build<FavouriteDealerRequest>()
            .With(x => x.BusinessLocationId, businessLocationId)
            .Create();

        // arrange
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync((Customer?) null);

        // act and assert
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await CustomerServiceObject().SaveFavouriteDealer(favouriteDealerRequest));
        Assert.Equal(StaticValues.CustomerNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorCustomerNotFound, exception.Message);
    }

    [Fact(DisplayName =
        "CustomerService: GetFavouriteDealer - Should get the favourite dealer of the customer.")]
    public async Task GetFavouriteDealer_Success()
    {
        // arrange
        const int businessLocationId = 1;
        Account customerAccountMock = _fixture.Build<Account>()
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, 1)
            .With(x => x.Account, customerAccountMock)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();

        DealerLocationProfileResponse favouriteDealerResponse = _fixture.Create<DealerLocationProfileResponse>();

        CustomerFavouriteDealerMapping customerFavouriteDealerMappingMock =
            _fixture.Build<CustomerFavouriteDealerMapping>()
                .With(x => x.CustomerId, customer.Id)
                .With(x => x.BusinessLocationId, businessLocationId)
                .Create();

        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerFavouriteDealerMappingRepository.Setup(x => x.GetCustomerFavouriteDealerMapping(1))
            .ReturnsAsync(customerFavouriteDealerMappingMock);
        _mockDealerService.Setup(x => x.GetDealerLocationProfile(businessLocationId))
            .ReturnsAsync(favouriteDealerResponse);

        // act
        DealerLocationProfileResponse? result = await CustomerServiceObject().GetFavouriteDealer();

        // assert
        Assert.NotNull(result);
        Assert.Equal(favouriteDealerResponse, result);
    }

    [Fact(DisplayName =
        "CustomerService: GetFavouriteDealer - Should return null if no favourite dealer exists for the customer.")]
    public async Task GetFavouriteDealer_NoFavouriteDealer_Success()
    {
        // arrange
        const int businessLocationId = 1;
        Account customerAccountMock = _fixture.Build<Account>()
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, 1)
            .With(x => x.Account, customerAccountMock)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();

        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerFavouriteDealerMappingRepository.Setup(x => x.GetCustomerFavouriteDealerMapping(1))
            .ReturnsAsync((CustomerFavouriteDealerMapping?) null);

        // act
        DealerLocationProfileResponse? result = await CustomerServiceObject().GetFavouriteDealer();

        // assert
        Assert.Null(result);
        _mockDealerService.Verify(x => x.GetDealerLocationProfile(businessLocationId), Times.Never);
    }

    [Fact(DisplayName =
        "CustomerService: GetDealerProfilesBySearchString - Should successfully get the list of all dealers whose business name starts with same value as businessSearchString input and which services the customer state.")]
    public async Task GetDealerProfilesByCustomerState_Success()
    {
        // arrange
        const string businessSearchString = "test";
        const string customerStateMock = "state";
        Account customerAccountMock = _fixture.Build<Account>()
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();

        Address addressMock = _fixture.Build<Address>()
            .With(x => x.State, customerStateMock)
            .With(x => x.IsPrimaryAddress, true)
            .Create();

        CustomerAddress customerAddressMock = _fixture.Build<CustomerAddress>()
            .With(x => x.Address, addressMock)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, 1)
            .With(x => x.Account, customerAccountMock)
            .With(x => x.CustomerAddresses, new List<CustomerAddress> {customerAddressMock})
            .Create();
        List<DealerProfileResponse> dealerProfileResponsesMock = _fixture.Create<List<DealerProfileResponse>>();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockDealerService.Setup(x => x.GetDealersBySearchStringAndState(businessSearchString, customerStateMock))
            .ReturnsAsync(dealerProfileResponsesMock);

        // act
        List<DealerProfileResponse> result =
            await CustomerServiceObject().GetDealerProfilesBySearchString(businessSearchString);

        // assert
        Assert.NotNull(result);
        Assert.Equal(dealerProfileResponsesMock, result);
        _mockDealerService.Verify(x => x.GetDealersBySearchStringAndState(businessSearchString, customerStateMock),
            Times.Once);
    }

    [Fact(DisplayName =
        "CustomerService: GetDealerProfilesBySearchString - Should throw business rule violation exception if the state could be found for the logged in customer.")]
    public async Task GetDealerProfilesByCustomerState_CustomerStateNotFound_ThrowsBusinessRuleViolationException()
    {
        // arrange
        const string businessSearchString = "test";
        Account customerAccountMock = _fixture.Build<Account>()
            .With(x => x.UserRole, UserRoleEnum.Customer)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, 1)
            .With(x => x.Account, customerAccountMock)
            .With(x => x.CustomerAddresses, new List<CustomerAddress>())
            .Create();
        _mockAccountService.Setup(x => x.GetAccountId(null)).ReturnsAsync(It.IsAny<int>());
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);

        // assert & act
        BusinessRuleViolationException exception =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await CustomerServiceObject().GetDealerProfilesBySearchString(businessSearchString));

        Assert.NotNull(exception);
        Assert.Equal(StaticValues.CustomerStateNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorCustomerStateNotFound, exception.Message);
        _mockDealerService.Verify(x => x.GetDealersBySearchStringAndState(businessSearchString, It.IsAny<string>()),
            Times.Never);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomerProfiles - Should throw business rule violation exception if the customers data do not present for the requested customer emails.")]
    public async Task GetCustomerProfilesByEmails_CustomersDataNotFound_ThrowsBusinessRuleViolationException()
    {
        // arrange
        CustomerByEmailRequest customerByEmailRequest = _fixture.Create<CustomerByEmailRequest>();
        _mockCustomerRepository.Setup(x => x.GetCustomersByEmailIds(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<Customer>());

        // assert & act
        BusinessRuleViolationException exception =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await CustomerServiceObject().GetCustomerProfiles(customerByEmailRequest));

        Assert.NotNull(exception);
        Assert.Equal(StaticValues.CustomerNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorCustomerNotFound, exception.Message);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomerProfiles - Should be able to get customers data along with favourite dealer for the requested customer emails.")]
    public async Task GetCustomerProfilesByEmails_CustomersDataExistsWithFavouriteDealer_Success()
    {
        // arrange
        const string customerEmail = "customer@mail.com";
        const string customerFirstName = "FirstName";
        const string customerLastName = "LastName";
        const string dealerEmail = "dealer@mail.com";
        const int accountId = 1;
        const int customerId = 2;
        const int businessLocationId = 3;
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, customerEmail)
            .With(x => x.Id, accountId)
            .With(x => x.FirstName, customerFirstName)
            .With(x => x.LastName, customerLastName)
            .Create();

        Address addressMock = _fixture.Create<Address>();

        CustomerAddress customerAddressMock = _fixture.Build<CustomerAddress>()
            .With(x => x.Address, addressMock)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, customerId)
            .With(x => x.Account, account)
            .With(x => x.CustomerAddresses, new List<CustomerAddress> {customerAddressMock})
            .Create();

        CustomerFavouriteDealerMapping customerFavouriteDealerMapping = _fixture.Build<CustomerFavouriteDealerMapping>()
            .With(x => x.CustomerId, customerId)
            .With(x => x.BusinessLocationId, businessLocationId).Create();

        BusinessResponse businessMock = _fixture.Build<BusinessResponse>()
            .With(business => business.Locations, new List<BusinessLocationResponse>
            {
                _fixture.Build<BusinessLocationResponse>()
                    .With(x => x.Id, businessLocationId)
                    .Create()
            })
            .Create();

        DealerResponse savedDealer = _fixture.Build<DealerResponse>()
            .With(x => x.Email, dealerEmail)
            .With(x => x.Business, businessMock).Create();

        CustomerByEmailRequest customerByEmailRequest = _fixture.Create<CustomerByEmailRequest>();
        _mockCustomerRepository.Setup(x => x.GetCustomersByEmailIds(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<Customer> {customer});
        _mockCustomerFavouriteDealerMappingRepository
            .Setup(x => x.GetCustomerFavouriteDealerMapping(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<CustomerFavouriteDealerMapping> {customerFavouriteDealerMapping});
        _mockDealerService.Setup(x => x.GetDealerByBusinessLocationIds(It.IsAny<List<int>>(), null))
            .ReturnsAsync(new List<DealerResponse> {savedDealer});

        // assert & act
        List<CustomerProfileByEmailResponse> results =
            await CustomerServiceObject().GetCustomerProfiles(customerByEmailRequest);

        Assert.NotNull(results);
        Assert.Equal(customerFirstName, results.FirstOrDefault()?.FirstName);
        Assert.Equal(customerLastName, results.FirstOrDefault()?.LastName);
        Assert.Equal(customerEmail, results.FirstOrDefault()?.Email);
        Assert.Equal(addressMock.State, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.State);
        Assert.Equal(addressMock.City, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.City);
        Assert.Equal(addressMock.ZipCode, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.ZipCode);
        Assert.Equal(addressMock.AddressValue, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.AddressValue);
        Assert.Equal(dealerEmail, results.FirstOrDefault()?.FavouriteDealer.Email);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomerProfiles - Should be able to get customers data without favourite dealer when favourite dealer data does not exist for the requested customer emails.")]
    public async Task GetCustomerProfilesByEmails_CustomersDataExistsWithoutFavouriteDealer_Success()
    {
        // arrange
        const string customerEmail = "customer@mail.com";
        const string customerFirstName = "FirstName";
        const string customerLastName = "LastName";
        const int accountId = 1;
        const int customerId = 2;
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, customerEmail)
            .With(x => x.Id, accountId)
            .With(x => x.FirstName, customerFirstName)
            .With(x => x.LastName, customerLastName)
            .Create();

        Address addressMock = _fixture.Create<Address>();

        CustomerAddress customerAddressMock = _fixture.Build<CustomerAddress>()
            .With(x => x.Address, addressMock)
            .Create();

        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Id, customerId)
            .With(x => x.Account, account)
            .With(x => x.CustomerAddresses, new List<CustomerAddress> {customerAddressMock})
            .Create();

        CustomerByEmailRequest customerByEmailRequest = _fixture.Create<CustomerByEmailRequest>();
        _mockCustomerRepository.Setup(x => x.GetCustomersByEmailIds(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<Customer> {customer});
        _mockCustomerFavouriteDealerMappingRepository
            .Setup(x => x.GetCustomerFavouriteDealerMapping(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<CustomerFavouriteDealerMapping>());
        _mockDealerService.Setup(x => x.GetDealerByBusinessLocationIds(It.IsAny<List<int>>(), null))
            .ReturnsAsync(new List<DealerResponse>());

        // assert & act
        List<CustomerProfileByEmailResponse> results =
            await CustomerServiceObject().GetCustomerProfiles(customerByEmailRequest);

        Assert.NotNull(results);
        Assert.Equal(customerFirstName, results.FirstOrDefault()?.FirstName);
        Assert.Equal(customerLastName, results.FirstOrDefault()?.LastName);
        Assert.Equal(customerEmail, results.FirstOrDefault()?.Email);
        Assert.Equal(addressMock.State, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.State);
        Assert.Equal(addressMock.City, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.City);
        Assert.Equal(addressMock.ZipCode, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.ZipCode);
        Assert.Equal(addressMock.AddressValue, results.FirstOrDefault()?.Addresses.FirstOrDefault()?.AddressValue);
        Assert.Null(results.FirstOrDefault()?.FavouriteDealer.Email);
    }

    [Fact(DisplayName =
        "CustomerService: CreateOrUpdateDeliveryInstructions - Should create new entry for delivery instructions when no entry exists.")]
    public async Task CreateOrUpdateDeliveryInstructions_SuccessNoExistingEntry()
    {
        // arrange
        DeliveryInstructionsRequest customerRegistrationRequest = _fixture.Create<DeliveryInstructionsRequest>();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(1);
        _mockCustomerDeliveryInstructionRepository.Setup(
                x => x.GetDeliveryInstruction(It.IsAny<string>()))
            .ReturnsAsync((CustomerDeliveryInstruction?) null);
        _mockCustomerDeliveryInstructionRepository.Setup(
                x => x.CreateDeliveryInstruction(It.IsAny<CustomerDeliveryInstruction>()))
            .ReturnsAsync(_fixture.Build<CustomerDeliveryInstruction>()
                .With(x => x.PetInformation, "cats")
                .Create());
        // act
        DeliveryInstructionsResponse result =
            await CustomerServiceObject().CreateOrUpdateDeliveryInstructions(customerRegistrationRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal("cats", result.PetInformation);
    }

    [Fact(DisplayName =
        "CustomerService: CreateOrUpdateDeliveryInstructions - Should update entry for delivery instructions when entry exists.")]
    public async Task CreateOrUpdateDeliveryInstructions_SuccessExistingEntry()
    {
        // arrange
        DeliveryInstructionsRequest customerRegistrationRequest = _fixture.Create<DeliveryInstructionsRequest>();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(1);
        _mockCustomerDeliveryInstructionRepository.Setup(
                x => x.GetDeliveryInstruction(It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<CustomerDeliveryInstruction>());
        _mockCustomerDeliveryInstructionRepository.Setup(
                x => x.UpdateDeliveryInstruction(It.IsAny<CustomerDeliveryInstruction>()))
            .ReturnsAsync(_fixture.Build<CustomerDeliveryInstruction>()
                .With(x => x.PetInformation, "dogs")
                .Create());
        // act
        DeliveryInstructionsResponse result =
            await CustomerServiceObject().CreateOrUpdateDeliveryInstructions(customerRegistrationRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal("dogs", result.PetInformation);
    }

    [Fact(DisplayName =
        "CustomerService: GetDeliveryInstructions - Should get delivery instructions when entry exists.")]
    public async Task GetDeliveryInstructions_SuccessExistingEntry()
    {
        // arrange
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(1);
        _mockCustomerDeliveryInstructionRepository.Setup(
                x => x.GetDeliveryInstruction(It.IsAny<string>()))
            .ReturnsAsync(_fixture.Build<CustomerDeliveryInstruction>()
                .With(x => x.SubdivisionName, "sdn")
                .Create());
        // act
        DeliveryInstructionsResponse result =
            await CustomerServiceObject().GetDeliveryInstructions();

        // assert
        Assert.NotNull(result);
        Assert.Equal("sdn", result.SubdivisionName);
    }

    [Fact(DisplayName =
        "CustomerService: CreateCustomerSuggestedDealers - Should create new entry when no previous entry exists.")]
    public async Task CreateCustomerSuggestedDealers_SuccessNoEntry()
    {
        // arrange
        EmailRequestWithTemplateParameters? emailRequestWithTemplateParameters = null;
        CustomerSuggestedDealerRequest customerRegistrationRequest = _fixture.Create<CustomerSuggestedDealerRequest>();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(1);
        _mockCustomerSuggestedDealerRepository.Setup(
                x => x.GetCustomerSuggestedDealer(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((CustomerSuggestedDealer?) null);
        _mockCustomerSuggestedDealerRepository.Setup(x =>
                x.CreateCustomerSuggestedDealer(It.IsAny<CustomerSuggestedDealer>()))
            .ReturnsAsync(_fixture.Build<CustomerSuggestedDealer>()
                .With(x => x.DealerEmail, "dealer@mail.com")
                .Create());
        _mockServiceBusConfig.Setup(x => x.SendEmailMessageToQueue
            (It.IsAny<string>())).Callback((string emailRequestAsString) =>
        {
            emailRequestWithTemplateParameters =
                JsonConvert.DeserializeObject<EmailRequestWithTemplateParameters>(emailRequestAsString);
        });
        // act
        CustomerSuggestedDealerResponse result =
            await CustomerServiceObject().CreateCustomerSuggestedDealers(customerRegistrationRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal("dealer@mail.com", result.DealerEmail);
        Assert.NotNull(emailRequestWithTemplateParameters);
        Assert.Equal("Customer Request to add a Dealer", emailRequestWithTemplateParameters?.Subject);
        Assert.Equal("InviteDealerEmailTemplate.html", emailRequestWithTemplateParameters?.HtmlTemplateName);
        Assert.Equal(customer.Account.FirstName,
            emailRequestWithTemplateParameters?.HtmlTemplateParameters.firstname.ToString());
    }

    [Fact(DisplayName =
        "CustomerService: CreateCustomerSuggestedDealers - Should update existing entry when previous entry exists.")]
    public async Task CreateCustomerSuggestedDealers_SuccessExistingEntry()
    {
        // arrange
        CustomerSuggestedDealerRequest customerRegistrationRequest = _fixture.Create<CustomerSuggestedDealerRequest>();
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        CustomerSuggestedDealer mock = _fixture.Build<CustomerSuggestedDealer>()
            .With(x => x.DealerEmail, "dealer@mail.com")
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(1);
        _mockCustomerSuggestedDealerRepository.Setup(
                x => x.GetCustomerSuggestedDealer(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mock);
        _mockCustomerSuggestedDealerRepository.Setup(x =>
                x.UpdateCustomerSuggestedDealer(It.IsAny<CustomerSuggestedDealer>()))
            .ReturnsAsync(mock);
        _mockServiceBusConfig.Setup(x => x.SendEmailMessageToQueue
                (It.IsAny<string>()))
            .Returns(Task.FromResult(true));
        // act
        CustomerSuggestedDealerResponse result =
            await CustomerServiceObject().CreateCustomerSuggestedDealers(customerRegistrationRequest);

        // assert
        Assert.NotNull(result);
        Assert.Equal(mock.DealerAddress, result.DealerAddress);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomers - Should be able to get customers list by list of emails")]
    public async Task GetCustomers_Success()
    {
        // arrange
        Account account = _fixture.Build<Account>()
            .With(x => x.Email, "abc@mail.com")
            .Create();
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.Account, account)
            .Create();
        List<Customer> customers = new() {customer};
        _mockCustomerRepository.Setup(x => x.GetCustomersByEmailIds(It.IsAny<List<string>>())).ReturnsAsync(customers);

        // act
        List<CustomerResponse> result =
            await CustomerServiceObject().GetCustomers(It.IsAny<List<string>>());

        // assert
        Assert.NotEmpty(result);
    }

    [Fact(DisplayName =
        "CustomerService: UploadCustomerProfilePhoto - Should return profile picture url on uploading profile photo.")]
    public async Task UploadCustomerProfilePhoto_Success()
    {
        //arrange
        ProfilePhotoUploadRequest profilePhotoUploadRequest =
            _fixture.Build<ProfilePhotoUploadRequest>()
                .With(x => x.ProfilePhoto, new Mock<IFormFile>().Object)
                .Create();
        Customer customer = _fixture.Build<Customer>().With(x => x.Id, 87).Create();
        string profilePhotoFileName = "87/Profile_Photo";
        BlobResponse blobResponse = _fixture.Build<BlobResponse>()
            .With(x => x.BlobUrl, "https://bloburl")
            .With(x => x.BlobId, 1234)
            .Create();

        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _catalogServiceClientMock.Setup(x => x.UploadFile(profilePhotoUploadRequest.ProfilePhoto, profilePhotoFileName,
            StaticValues.CustomersContainerName,
            customer.ProfilePhotoBlobId)).ReturnsAsync(blobResponse);

        //act 
        ProfilePhotoUploadResponse response =
            await CustomerServiceObject().UploadCustomerProfilePhoto(profilePhotoUploadRequest);

        //assert
        Assert.Equal(customer.ProfilePhotoBlobId, blobResponse.BlobId);
        Assert.Equal(response.ProfilePhotoUrl, blobResponse.BlobUrl);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomerIdByEmail - Should be able to get the customer id for the given email.")]
    public async Task GetCustomerIdByEmail_Success()
    {
        // arrange
        const string customerEmail = "test@mail.com";
        const int customerId = 1;
        _mockCustomerRepository.Setup(x => x.GetCustomerIdByEmail(customerEmail)).ReturnsAsync(customerId);

        // act
        int result = await CustomerServiceObject().GetCustomerIdByEmail(customerEmail);

        // assert
        Assert.Equal(customerId, result);
    }

    [Fact(DisplayName =
        "CustomerService: GetCustomerIdByEmail - Should throw not found exception if the customer could not found for the given email.")]
    public async Task GetCustomerIdByEmail_CustomerNotFound_ThrowsNotFoundException()
    {
        // arrange
        const string customerEmail = "test@mail.com";
        _mockCustomerRepository.Setup(x => x.GetCustomerIdByEmail(customerEmail))
            .ReturnsAsync((int?) null);

        // assert & act
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await CustomerServiceObject().GetCustomerIdByEmail(customerEmail));

        Assert.NotNull(exception);
        Assert.Equal(StaticValues.CustomerNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ErrorCustomerNotFound, exception.Message);
    }

    [Fact(DisplayName =
        "CustomerService: UpdateFirstCallAvailed - Should be able to update FirstFreeCallAvailed column.")]
    public async Task UpdateFirstCallAvailed_Success()
    {
        // arrange
        Customer customer = _fixture.Build<Customer>()
            .Create();
        _mockCustomerService.Setup(x => x.GetCustomerByAccountId(It.IsAny<string>())).ReturnsAsync(customer);
        _mockAccountService.Setup(x => x.GetAccountId(It.IsAny<string>())).ReturnsAsync(1);
        _mockCustomerRepository.Setup(x => x.GetCustomerByAccountId(It.IsAny<int>())).ReturnsAsync(customer);
        _mockCustomerRepository.Setup(x => x.UpdateCustomer(customer)).ReturnsAsync(customer);

        //act 
        await CustomerServiceObject().UpdateFirstFreeCallAvailed(It.IsAny<string>(), It.IsAny<bool>());

        //assert
        _mockCustomerRepository.Verify(x => x.UpdateCustomer(customer), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerService: GetFirstCallAvailed - Should be able to get FirstFreeCallAvailed status.")]
    public async Task GetFirstCallAvailed_Success()
    {
        // arrange
        const string email = "customer@customer.com";
        _mockAccountRepository.Setup(x => x.GetFreeCallStatusByEmail(email)).ReturnsAsync(true);

        // act
        bool? result =
            await CustomerServiceObject().GetFirstFreeCallAvailed(email);

        // assert
        Assert.NotNull(result);
        Assert.True(result);
    }

    [Fact(DisplayName =
        "CustomerService: GetFirstCallAvailed - throw the unauthorized exception when email in header isn't available.")]
    public async Task GetFirstCallAvailed_Unauthorized()
    {
        // arrange
        const string email = "";
        _mockAccountRepository.Setup(x => x.GetFreeCallStatusByEmail(email)).ReturnsAsync(true);

        // act
        UnauthorizedException? result =
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await CustomerServiceObject().GetFirstFreeCallAvailed(email));
        // assert
        Assert.NotNull(result);
        Assert.Equal("You are not authorized!", result.Message);
    }

    [Fact(DisplayName =
        "CustomerService: DeleteCustomer - Should throw the unauthorized exception when customer could not be found for the email in header.")]
    public async Task DeleteCustomer_ThrowsUnauthorizedException()
    {
        // arrange
        const string email = "customer@mail.com";
        _mockCustomerRepository.Setup(x => x.GetCustomerByEmail(email))
            .ReturnsAsync((Customer?) null);

        // act
        UnauthorizedException? result =
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await CustomerServiceObject().DeleteCustomer(email));
        // assert
        Assert.NotNull(result);
        Assert.Equal("You are not authorized!", result.Message);
        _mockCustomerRepository.Verify(x => x.GetCustomerByEmail(email), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerService: DeleteCustomer - Should throw the business rule violation exception when customer could not be found for the email in header.")]
    public async Task DeleteCustomer_ThrowsBusinessRuleViolationException()
    {
        // arrange
        const string email = "customer@mail.com";
        Customer deletedCustomer = _fixture.Build<Customer>()
            .With(x => x.IsDeleted, true)
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByEmail(email))
            .ReturnsAsync(deletedCustomer);

        // act
        BusinessRuleViolationException? result =
            await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
                await CustomerServiceObject().DeleteCustomer(email));
        // assert
        Assert.NotNull(result);
        Assert.Equal(StaticValues.ErrorCustomerAlreadyDeleted, result.Message);
        Assert.Equal(StaticValues.CustomerAlreadyDeleted, result.ErrorResponseType);
        _mockCustomerRepository.Verify(x => x.GetCustomerByEmail(email), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerService: DeleteCustomer - Should be able to successfully mark the customer account as deleted.")]
    public async Task DeleteCustomer_Success()
    {
        // arrange
        const string email = "customer@mail.com";
        Customer customer = _fixture.Build<Customer>()
            .With(x => x.IsDeleted, false)
            .With(x => x.ProfilePhotoBlobId, 1)
            .Create();
        _mockCustomerRepository.Setup(x => x.GetCustomerByEmail(email))
            .ReturnsAsync(customer);
        _catalogServiceClientMock.Setup(x => x.DeleteBlobById(1));
        _mockCustomerRepository.Setup(x => x.DeleteCustomerAccount(customer));
        _mockServiceBusConfig.Setup(x => x.SendDeleteAccountMessageToTopic(It.IsAny<string>()));
        // act
        await CustomerServiceObject().DeleteCustomer(email);
        // assert
        _mockCustomerRepository.Verify(x => x.GetCustomerByEmail(email), Times.Once);
        _catalogServiceClientMock.Verify(x => x.DeleteBlobById(1), Times.Once);
        _mockCustomerRepository.Verify(x => x.DeleteCustomerAccount(customer), Times.Once);
        _mockServiceBusConfig.Verify(x => x.SendDeleteAccountMessageToTopic(It.IsAny<string>()), Times.Once);
    }

    private CustomerService CustomerServiceObject()
    {
        return new CustomerService(
            _mockAccountService.Object,
            _mockAddressService.Object,
            _mockAuthService.Object,
            _mockDealerService.Object,
            _mockPaymentServiceClient.Object,
            _mockAddressRepository.Object,
            _mockAccountRepository.Object,
            _mockCustomerDeliveryInstructionRepository.Object,
            _mockCustomerFavouriteDealerMappingRepository.Object,
            _mockCustomerRepository.Object,
            _mockDistributedCache.Object,
            _mapper,
            _mockAppConfig.Object,
            _mockServiceBusConfig.Object,
            _mockCustomerSuggestedDealerRepository.Object,
            _catalogServiceClientMock.Object,
            _mockCustomerServiceClient.Object,
            _userManager.Object
        );
    }
}
