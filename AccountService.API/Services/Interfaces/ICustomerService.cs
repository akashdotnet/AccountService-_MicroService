using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Dto;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;

namespace AccountService.API.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponse> CreateCustomer(CustomerRegistrationRequest customerRegistrationRequest, Session session);
    Task<CustomerResponse> UpdateCustomer(UpdateCustomerRequest updateCustomerRequest);

    Task UpdateCustomerNotificationPreferences(
        NotificationPreferenceRequest notificationPreferenceRequest);

    Task<NotificationPreferenceResponse> GetNotificationPreferences();
    Task<CustomerResponse> GetCustomer(string? email = null);
    Task<Session> CreateCustomerSession(CustomerRegistrationRequest customerRegistrationRequest);
    Task<CustomerSession> GetCustomerSession(string sessionId);

    Task<CustomerRegistrationRequest> GetCustomerRegistrationRequestAndValidateEmailVerification(Session session);
    Task<DealerLocationProfileResponse> SaveFavouriteDealer(FavouriteDealerRequest favouriteDealerRequest);
    Task<Customer> GetCustomerByAccountId(string? email = null);
    Task<DealerLocationProfileResponse?> GetFavouriteDealer();
    Task<List<DealerProfileResponse>> GetDealerProfilesBySearchString(string businessSearchString);
    Task<List<CustomerProfileByEmailResponse>> GetCustomerProfiles(CustomerByEmailRequest customerByEmailRequest);

    Task<DeliveryInstructionsResponse> CreateOrUpdateDeliveryInstructions(
        DeliveryInstructionsRequest deliveryInstructionsRequest);

    Task<DeliveryInstructionsResponse> GetDeliveryInstructions();

    Task<CustomerSuggestedDealerResponse> CreateCustomerSuggestedDealers(
        CustomerSuggestedDealerRequest customerSuggestedDealerRequest);

    Task<ProfilePhotoUploadResponse> UploadCustomerProfilePhoto(ProfilePhotoUploadRequest profilePhotoUploadRequest);
    Task DeleteCustomer(string customerEmail);
    Task<List<CustomerResponse>> GetCustomers(List<string> emails);
    Task<int> GetCustomerIdByEmail(string email);
    Task UpdateFirstFreeCallAvailed(string email, bool? status);
    Task<bool> GetFirstFreeCallAvailed(string email);
    Task DeleteCustomerProfilePicture(string email);
}
