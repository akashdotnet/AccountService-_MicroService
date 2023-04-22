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
using AccountService.API.Services.Interfaces;
using AccountService.API.Utils.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Enums;
using PodCommonsLibrary.Core.Exceptions;

namespace AccountService.API.Services;

public class CustomerService : ICustomerService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountService _accountService;
    private readonly IAddressRepository _addressRepository;
    private readonly IAddressService _addressService;
    private readonly AppConfig _appConfig;
    private readonly IAuthService _authService;
    private readonly IDistributedCache _cache;
    private readonly ICatalogServiceClient _catalogServiceClient;
    private readonly ICustomerDeliveryInstructionRepository _customerDeliveryInstructionRepository;
    private readonly ICustomerFavouriteDealerMappingRepository _customerFavouriteDealerMappingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly ICustomerSuggestedDealerRepository _customerSuggestedDealerRepository;
    private readonly IDealerService _dealerService;
    private readonly IMapper _mapper;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IServiceBusConfig _serviceBusConfig;

    public CustomerService(
        IAccountService accountService,
        IAddressService addressService,
        IAuthService authService,
        IDealerService dealerService,
        IPaymentServiceClient paymentServiceClient,
        IAddressRepository addressRepository,
        IAccountRepository accountRepository,
        ICustomerDeliveryInstructionRepository customerDeliveryInstructionRepository,
        ICustomerFavouriteDealerMappingRepository customerFavouriteDealerMappingRepository,
        ICustomerRepository customerRepository,
        IDistributedCache cache,
        IMapper mapper,
        AppConfig appConfig,
        IServiceBusConfig serviceBusConfig,
        ICustomerSuggestedDealerRepository customerSuggestedDealerRepository,
        ICatalogServiceClient catalogServiceClient,
        ICustomerServiceClient customerServiceClient,
        UserManager<IdentityUser> userManager
    )
    {
        _accountService = accountService;
        _addressService = addressService;
        _authService = authService;
        _dealerService = dealerService;
        _addressRepository = addressRepository;
        _customerDeliveryInstructionRepository = customerDeliveryInstructionRepository;
        _customerFavouriteDealerMappingRepository = customerFavouriteDealerMappingRepository;
        _customerSuggestedDealerRepository = customerSuggestedDealerRepository;
        _customerRepository = customerRepository;
        _cache = cache;
        _mapper = mapper;
        _appConfig = appConfig;
        _serviceBusConfig = serviceBusConfig;
        _paymentServiceClient = paymentServiceClient;
        _catalogServiceClient = catalogServiceClient;
        _customerServiceClient = customerServiceClient;
        _accountRepository = accountRepository;
    }

    public async Task<CustomerResponse> CreateCustomer(CustomerRegistrationRequest customerRegistrationRequest,
        Session session)
    {
        Customer? customer = await _customerRepository.GetCustomerByEmail(customerRegistrationRequest.Email);
        if (customer is not null && customer.IsDeleted is not true)
        {
            throw new BusinessRuleViolationException(StaticValues.UserWithSameEmailExists,
                StaticValues.ErrorUserRegister);
        }

        customer ??= new Customer
        {
            Account = new Account()
        };
        _mapper.Map(customerRegistrationRequest, customer.Account);
        customer.LastCompletedOnboardingStep = CustomerStepEnum.SignUpComplete;
        customer.Account.UserRole = UserRoleEnum.Customer;
        customer.ReceivePromotionalContent = false;
        customer.FirstFreeCallAvailed = false;
        customer.PasswordResetDate = DateTime.UtcNow.Date;
        customer.IsDeleted = false;
        Customer savedCustomer = customer.Id == 0
            ? await _customerRepository.CreateCustomer(customer)
            : await _customerRepository.UpdateCustomer(customer);
        await _cache.RemoveAsync(session.SessionId);
        return _mapper.Map<CustomerResponse>(savedCustomer);
    }

    public async Task<CustomerResponse> UpdateCustomer(UpdateCustomerRequest updateCustomerRequest)
    {
        Customer customer = await GetCustomerByAccountId();
        _mapper.Map(updateCustomerRequest, customer);
        _mapper.Map(updateCustomerRequest, customer.Account);
        if (updateCustomerRequest.OnboardingStep != CustomerStepEnum.EditProfile)
        {
            CustomerStepEnum currentCustomerStep = updateCustomerRequest.OnboardingStep;
            if (currentCustomerStep > customer.LastCompletedOnboardingStep)
            {
                customer.LastCompletedOnboardingStep = currentCustomerStep;
            }

            customer.Account.IsOnboardingComplete =
                customer.LastCompletedOnboardingStep == CustomerStepEnum.PoolDetails;
            if (customer.Account.IsOnboardingComplete)
            {
                // create customer account in stripe
                CreateCustomerForPaymentRequest createCustomerForPaymentRequest =
                    _mapper.Map<CreateCustomerForPaymentRequest>(customer);
                await _paymentServiceClient.CreateCustomer(createCustomerForPaymentRequest);

                // create customer reminders from catalog reminders
                CreateCustomerReminderRequest createCustomerReminderRequest = new()
                {
                    Email = customer.Account.Email,
                    StartDate = DateTime.Now
                };
                await _customerServiceClient.CreateCustomerReminders(createCustomerReminderRequest);
            }
        }

        if (updateCustomerRequest.Address != null)
        {
            customer = await CreateOrUpdateCustomerAddresses(customer, updateCustomerRequest.Address);
        }

        Customer savedCustomer = await _customerRepository.UpdateCustomer(customer);

        CustomerResponse customerResponse = _mapper.Map<CustomerResponse>(savedCustomer);
        savedCustomer.CustomerAddresses.ForEach(address =>
        {
            CustomerAddressResponse customerAddressResponse = _mapper.Map<CustomerAddressResponse>(address);
            customerResponse.Addresses.Add(customerAddressResponse);
        });
        return customerResponse;
    }

    public async Task UpdateCustomerNotificationPreferences(
        NotificationPreferenceRequest notificationPreferenceRequest)
    {
        Customer customer = await GetCustomerByAccountId();
        Customer updatedCustomer = _mapper.Map(notificationPreferenceRequest, customer);
        await _customerRepository.UpdateCustomer(updatedCustomer);
    }

    public async Task<NotificationPreferenceResponse> GetNotificationPreferences()
    {
        Customer customer = await GetCustomerByAccountId();
        return _mapper.Map<NotificationPreferenceResponse>(customer);
    }

    public async Task<CustomerResponse> GetCustomer(string? email = null)
    {
        Customer customer = await GetCustomerByAccountId(email);
        CustomerResponse customerResponse = _mapper.Map<CustomerResponse>(customer);
        customer.CustomerAddresses.ForEach(address =>
        {
            CustomerAddressResponse customerAddressResponse = _mapper.Map<CustomerAddressResponse>(address);
            customerResponse.Addresses.Add(customerAddressResponse);
        });
        int? profilePhotoBlobId = customer.ProfilePhotoBlobId;
        if (profilePhotoBlobId != null)
        {
            BlobResponse blobResponse = await _catalogServiceClient.GetBlobUrl((int) profilePhotoBlobId);
            customerResponse.ProfilePhotoUrl = blobResponse.BlobUrl;
        }

        return customerResponse;
    }

    public async Task<Session> CreateCustomerSession(CustomerRegistrationRequest customerRegistrationRequest)
    {
        // check if user already exists
        await _authService.CheckIfUserExists(customerRegistrationRequest.Email);

        string sessionId = Guid.NewGuid().ToString();
        CustomerSession customerSession = new()
        {
            EmailVerified = false,
            CustomerRegistrationRequest = customerRegistrationRequest,
            Type = EmailTemplateEnum.Register,
            Email = customerRegistrationRequest.Email
        };
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_appConfig.SessionValidityInDays)
        };
        await _cache.SetAsync(sessionId, customerSession.ObjectToByteArray(), options);

        return new Session
        {
            SessionId = sessionId
        };
    }

    public async Task<CustomerRegistrationRequest> GetCustomerRegistrationRequestAndValidateEmailVerification(
        Session session)
    {
        CustomerSession customerSession = await GetCustomerSession(session.SessionId);
        if (!customerSession.EmailVerified)
        {
            throw new BusinessRuleViolationException(StaticValues.EmailNotVerified,
                StaticValues.ErrorEmailNotVerifiedBeforeRegistration);
        }

        return customerSession.CustomerRegistrationRequest;
    }

    public async Task<CustomerSession> GetCustomerSession(string sessionId)
    {
        byte[] customerSessionByteArray = await _cache.GetAsync(sessionId);
        CustomerSession? customerSession = customerSessionByteArray?.ByteArrayToObject<CustomerSession>();
        if (customerSession == null)
        {
            throw new BusinessRuleViolationException(StaticValues.InvalidSession,
                StaticValues.ErrorInvalidSession(sessionId));
        }

        return customerSession;
    }

    public async Task<DealerLocationProfileResponse> SaveFavouriteDealer(FavouriteDealerRequest favouriteDealerRequest)
    {
        Customer customer = await GetCustomerByAccountId();
        (Dealer? dealer, BusinessLocation businessLocation) =
            await _dealerService.GetDealerAndBusinessLocationById(favouriteDealerRequest.BusinessLocationId);
        CustomerFavouriteDealerMapping customerFavouriteDealerMapping =
            await _customerFavouriteDealerMappingRepository.GetCustomerFavouriteDealerMapping(customer.Id) ??
            new CustomerFavouriteDealerMapping {Customer = customer};
        customerFavouriteDealerMapping.BusinessLocation = businessLocation;
        await _customerFavouriteDealerMappingRepository.CreateOrUpdateCustomerFavouriteDealerMapping(
            customerFavouriteDealerMapping);
        return await _dealerService.MapDealerAndBusinessLocationToDealerLocationResponse(dealer, businessLocation);
    }

    public async Task<Customer> GetCustomerByAccountId(string? email = null)
    {
        int accountId = await _accountService.GetAccountId(email);
        Customer? customer = await _customerRepository.GetCustomerByAccountId(accountId);
        if (customer == null)
        {
            throw new NotFoundException(StaticValues.CustomerNotFound, StaticValues.ErrorCustomerNotFound);
        }

        return customer;
    }

    public async Task<DealerLocationProfileResponse?> GetFavouriteDealer()
    {
        Customer customer = await GetCustomerByAccountId();
        CustomerFavouriteDealerMapping? customerFavouriteDealerMapping =
            await _customerFavouriteDealerMappingRepository.GetCustomerFavouriteDealerMapping(customer.Id);
        if (customerFavouriteDealerMapping == null)
        {
            return null;
        }

        return await _dealerService.GetDealerLocationProfile(customerFavouriteDealerMapping.BusinessLocationId);
    }

    public async Task<List<DealerProfileResponse>> GetDealerProfilesBySearchString(string businessSearchString)
    {
        Customer customer = await GetCustomerByAccountId();
        CustomerAddress? customerPrimaryAddress = customer.CustomerAddresses.Find(x => x.Address.IsPrimaryAddress);
        string? customerState = customerPrimaryAddress?.Address.State;
        if (customerState == null)
        {
            throw new BusinessRuleViolationException(StaticValues.CustomerStateNotFound,
                StaticValues.ErrorCustomerStateNotFound);
        }

        List<DealerProfileResponse> dealers =
            await _dealerService.GetDealersBySearchStringAndState(businessSearchString, customerState);
        return dealers;
    }

    public async Task<List<CustomerProfileByEmailResponse>> GetCustomerProfiles(
        CustomerByEmailRequest customerByEmailRequest)
    {
        // get all the customer details from emails
        List<Customer> customers =
            await _customerRepository.GetCustomersByEmailIds(customerByEmailRequest.Emails);
        if (!customers.Any())
        {
            throw new BusinessRuleViolationException(StaticValues.CustomerNotFound,
                StaticValues.ErrorCustomerNotFound);
        }

        List<int> blobIds = customers.Aggregate(new List<int>(), (blobIdList, next) =>
        {
            if (next.ProfilePhotoBlobId != null)
            {
                blobIdList.Add((int) next.ProfilePhotoBlobId);
            }

            return blobIdList;
        });
        List<BlobResponse> blobResponses = blobIds.IsNullOrEmpty()
            ? new List<BlobResponse>()
            : await _catalogServiceClient.GetBlobs(blobIds);
        Dictionary<int, string> blobIdAndResponseMap = blobResponses.IsNullOrEmpty()
            ? new Dictionary<int, string>()
            : blobResponses.ToDictionary(x => x.BlobId, x => x.BlobUrl);


        // Get favourite dealer's business location Id of those customers
        List<CustomerFavouriteDealerMapping> customerFavouriteDealerMappings =
            await _customerFavouriteDealerMappingRepository.GetCustomerFavouriteDealerMapping(customers
                .Select(x => x.Id)
                .ToList());

        List<DealerResponse> dealers = new();

        // Get Dealer details on the basis of business location Ids
        if (customerFavouriteDealerMappings.Any())
        {
            dealers =
                await _dealerService.GetDealerByBusinessLocationIds(customerFavouriteDealerMappings
                    .Select(x => x.BusinessLocationId).ToList());
        }

        return MapCustomerAddressAndFavouriteDealerInfo(customers, dealers, customerFavouriteDealerMappings,
            blobIdAndResponseMap);
    }

    public async Task<DeliveryInstructionsResponse> CreateOrUpdateDeliveryInstructions(
        DeliveryInstructionsRequest deliveryInstructionsRequest)
    {
        Customer customer = await GetCustomerByAccountId();
        CustomerDeliveryInstruction? existingInstruction =
            await _customerDeliveryInstructionRepository.GetDeliveryInstruction(customer.Account.Email);
        CustomerDeliveryInstruction? savedInstruction;
        if (existingInstruction == null)
        {
            CustomerDeliveryInstruction newInstruction =
                _mapper.Map<CustomerDeliveryInstruction>(deliveryInstructionsRequest);
            newInstruction.Customer = customer;
            savedInstruction = await _customerDeliveryInstructionRepository.CreateDeliveryInstruction(newInstruction);
        }
        else
        {
            existingInstruction = _mapper.Map(deliveryInstructionsRequest, existingInstruction);
            savedInstruction =
                await _customerDeliveryInstructionRepository.UpdateDeliveryInstruction(existingInstruction);
        }

        return _mapper.Map<DeliveryInstructionsResponse>(savedInstruction);
    }

    public async Task<DeliveryInstructionsResponse> GetDeliveryInstructions()
    {
        Customer customer = await GetCustomerByAccountId();
        CustomerDeliveryInstruction? customerDeliveryInstruction =
            await _customerDeliveryInstructionRepository.GetDeliveryInstruction(customer.Account.Email);
        return customerDeliveryInstruction == null
            ? new DeliveryInstructionsResponse()
            : _mapper.Map<DeliveryInstructionsResponse>(customerDeliveryInstruction);
    }

    public async Task<CustomerSuggestedDealerResponse> CreateCustomerSuggestedDealers(
        CustomerSuggestedDealerRequest customerSuggestedDealerRequest)
    {
        Customer customer = await GetCustomerByAccountId();
        CustomerSuggestedDealer? existingDealer =
            await _customerSuggestedDealerRepository.GetCustomerSuggestedDealer(customerSuggestedDealerRequest
                .DealerEmail, customer.Account.Email);
        CustomerSuggestedDealer? savedDealer;
        if (existingDealer == null)
        {
            CustomerSuggestedDealer customerSuggestedDealer =
                _mapper.Map<CustomerSuggestedDealer>(customerSuggestedDealerRequest);
            customerSuggestedDealer.Customer = customer;
            savedDealer =
                await _customerSuggestedDealerRepository.CreateCustomerSuggestedDealer(customerSuggestedDealer);
        }
        else
        {
            existingDealer = _mapper.Map(customerSuggestedDealerRequest, existingDealer);
            savedDealer = await _customerSuggestedDealerRepository.UpdateCustomerSuggestedDealer(existingDealer);
        }

        await SendMailForSuggestedDealer(customer, customerSuggestedDealerRequest);
        return _mapper.Map<CustomerSuggestedDealerResponse>(savedDealer);
    }

    public async Task<List<CustomerResponse>> GetCustomers(List<string> emails)
    {
        List<Customer> customers = await _customerRepository.GetCustomersByEmailIds(emails);
        List<CustomerResponse> customerResponse = _mapper.Map<List<CustomerResponse>>(customers);
        customers.ForEach(customer =>
        {
            List<CustomerAddressResponse>? customerAddressResponse =
                _mapper.Map<List<CustomerAddressResponse>>(customer.CustomerAddresses);
            customerResponse.FirstOrDefault(x => x.Email == customer.Account.Email)
                ?.Addresses
                .AddRange(customerAddressResponse);
        });
        return customerResponse;
    }

    public async Task UpdateFirstFreeCallAvailed(string email, bool? status)
    {
        Customer customer = await GetCustomerByAccountId(email);
        customer.FirstFreeCallAvailed = status ?? false;
        await _customerRepository.UpdateCustomer(customer);
    }

    public async Task<bool> GetFirstFreeCallAvailed(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedException();
        }

        return await _accountRepository.GetFreeCallStatusByEmail(email);
    }

    public async Task DeleteCustomerProfilePicture(string email)
    {
        Customer customer = await GetCustomerByAccountId(email);
        if (customer.ProfilePhotoBlobId == null)
        {
            throw new BusinessRuleViolationException(StaticValues.ProfilePictureNotFound,
                StaticValues.ErrorProfilePictureNotFound);
        }

        await _catalogServiceClient.DeleteBlobById((int) customer.ProfilePhotoBlobId);
        customer.ProfilePhotoBlobId = null;
        await _customerRepository.UpdateCustomer(customer);
    }

    public async Task<ProfilePhotoUploadResponse> UploadCustomerProfilePhoto(
        ProfilePhotoUploadRequest profilePhotoUploadRequest)
    {
        Customer customer = await GetCustomerByAccountId();

        string profilePhotoFileName =
            StaticValues.GetCustomerProfilePhotoFileName(profilePhotoUploadRequest.ProfilePhoto, customer.Id);
        BlobResponse blobResponse = await _catalogServiceClient.UploadFile(
            profilePhotoUploadRequest.ProfilePhoto, profilePhotoFileName, StaticValues.CustomersContainerName,
            customer.ProfilePhotoBlobId);
        customer.ProfilePhotoBlobId = blobResponse.BlobId;
        await _customerRepository.UpdateCustomer(customer);
        return new ProfilePhotoUploadResponse
        {
            ProfilePhotoUrl = blobResponse.BlobUrl
        };
    }

    public async Task DeleteCustomer(string customerEmail)
    {
        Customer? customer = await _customerRepository.GetCustomerByEmail(customerEmail);
        if (customer == null)
        {
            throw new UnauthorizedException();
        }

        if (customer.IsDeleted)
        {
            throw new BusinessRuleViolationException(
                StaticValues.CustomerAlreadyDeleted,
                StaticValues.ErrorCustomerAlreadyDeleted
            );
        }

        DeleteAccountRequest deleteAccountRequest = new()
        {
            CustomerEmail = customerEmail,
            CustomerId = customer.Id
        };
        await _customerRepository.DeleteCustomerAccount(customer);
        if (customer.ProfilePhotoBlobId != null)
        {
            await _catalogServiceClient.DeleteBlobById((int) customer.ProfilePhotoBlobId);
        }

        string message = JsonConvert.SerializeObject(deleteAccountRequest);

        await _serviceBusConfig.SendDeleteAccountMessageToTopic(message);
    }

    public async Task<int> GetCustomerIdByEmail(string email)
    {
        int? customerId = await _customerRepository.GetCustomerIdByEmail(email);
        if (customerId is 0 or null)
        {
            throw new NotFoundException(StaticValues.CustomerNotFound, StaticValues.ErrorCustomerNotFound);
        }

        return customerId.Value;
    }

    private async Task SendMailForSuggestedDealer(Customer customer,
        CustomerSuggestedDealerRequest customerSuggestedDealerRequest)
    {
        dynamic inviteDealerEmailParams = new
        {
            firstname = customer.Account.FirstName,
            lastname = customer.Account.LastName,
            dealername = customerSuggestedDealerRequest.DealerName,
            dealeraddress = customerSuggestedDealerRequest.DealerAddress,
            dealeremail = customerSuggestedDealerRequest.DealerEmail
        };
        EmailRequestWithTemplateParameters emailRequestWithTemplateParameters = new()
        {
            RecipientAddress = _appConfig.DstEmailAddress,
            SenderAddress = _appConfig.OtpEmailSenderAddress,
            Subject = StaticValues.InviteDealerSubject,
            HtmlTemplateName = StaticValues.InviteDealerEmailTemplateName,
            HtmlTemplateParameters = inviteDealerEmailParams
        };
        string message = JsonConvert.SerializeObject(emailRequestWithTemplateParameters);
        await _serviceBusConfig.SendEmailMessageToQueue(message);
    }

    private List<CustomerProfileByEmailResponse> MapCustomerAddressAndFavouriteDealerInfo(List<Customer> customers,
        List<DealerResponse> dealers,
        List<CustomerFavouriteDealerMapping> customerFavouriteDealerMappings, Dictionary<int, string> blobIdAndUrlMap)
    {
        List<CustomerProfileByEmailResponse> customerResponse =
            _mapper.Map<List<CustomerProfileByEmailResponse>>(customers);
        // mapping address and favourite dealer info for all the customers
        customers.ForEach(customer =>
        {
            customer.CustomerAddresses.ForEach(address =>
            {
                CustomerAddressResponse customerAddressResponse = _mapper.Map<CustomerAddressResponse>(address);
                customerResponse.First(x => x.Email == customer.Account.Email).Addresses
                    .Add(customerAddressResponse);
            });

            int? businessLocationId = customerFavouriteDealerMappings.FirstOrDefault(x => x.CustomerId == customer.Id)?
                .BusinessLocationId;
            customerResponse.First(x => x.Email == customer.Account.Email).FavouriteDealer =
                new CustomerProfileFavouriteDealerResponse
                {
                    Email = businessLocationId == null
                        ? null
                        : dealers
                            .FirstOrDefault(x =>
                                x.Business != null &&
                                x.Business.Locations.Select(l => l.Id).Contains(Convert.ToInt32(businessLocationId)))?
                            .Email
                };
            customerResponse.First(x => x.Email == customer.Account.Email).ProfilePhotoUrl =
                customer.ProfilePhotoBlobId != null && blobIdAndUrlMap.ContainsKey((int) customer.ProfilePhotoBlobId)
                    ? blobIdAndUrlMap[(int) customer.ProfilePhotoBlobId]
                    : null;
        });

        return customerResponse;
    }

    private async Task<Customer> CreateOrUpdateCustomerAddresses(Customer customer,
        CustomerAddressRequest addressRequest)
    {
        // Validate Address for state and state-zipcode mapping
        await _addressService.ValidateAddress(addressRequest);

        if (customer.CustomerAddresses.Count > 0)
        {
            customer.CustomerAddresses[0].Address =
                _mapper.Map(addressRequest, customer.CustomerAddresses[0].Address);
            customer.CustomerAddresses[0].Address.IsPrimaryAddress = true;
        }
        else
        {
            Address address = _mapper.Map<Address>(addressRequest);
            CustomerAddress customerAddress = new()
            {
                Address = await _addressRepository.CreateOrUpdateAddress(address),
                Customer = customer
            };
            customer.CustomerAddresses.Clear();
            customerAddress.Address.IsPrimaryAddress = true;
            customer.CustomerAddresses.Add(customerAddress);
        }

        return customer;
    }
}
