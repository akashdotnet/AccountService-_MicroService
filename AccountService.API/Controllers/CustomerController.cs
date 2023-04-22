using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PodCommonsLibrary.Core.Annotations;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Controllers;

[ApiController]
[Route(StaticValues.CustomersControllerRoutePrefix)]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IDealerService _dealerService;
    private readonly IExpertService _expertService;

    public CustomerController(ICustomerService customerService, IExpertService expertService,
        IDealerService dealerService)
    {
        _customerService = customerService;
        _expertService = expertService;
        _dealerService = dealerService;
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpPut]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<CustomerResponse> UpdateCustomer([FromBody] UpdateCustomerRequest updateCustomerRequest)
    {
        return await _customerService.UpdateCustomer(updateCustomerRequest);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<CustomerResponse> GetCustomer()
    {
        return await _customerService.GetCustomer();
    }

    [HttpGet(StaticValues.CustomerByEmailInternalPath)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<CustomerResponse> GetCustomerByEmail(string email)
    {
        return await _customerService.GetCustomer(email);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpPut(StaticValues.NotificationPreferencesPath)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task UpdateCustomerNotificationPreferences(
        [FromBody] NotificationPreferenceRequest notificationPreferenceRequest)
    {
        await _customerService.UpdateCustomerNotificationPreferences(notificationPreferenceRequest);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet(StaticValues.NotificationPreferencesPath)]
    [ProducesResponseType(typeof(NotificationPreferenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<NotificationPreferenceResponse> GetNotificationPreferences()
    {
        return await _customerService.GetNotificationPreferences();
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet(StaticValues.ExpertProfilesPath)]
    [ProducesResponseType(typeof(List<ExpertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<ExpertResponse>> GetAllExpertProfiles()
    {
        return await _expertService.GetAllExpertProfiles();
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpPost(StaticValues.FavouriteDealerPath)]
    [ProducesResponseType(typeof(DealerLocationProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DealerLocationProfileResponse> SaveFavouriteDealer(
        [FromBody] FavouriteDealerRequest favouriteDealerRequest)
    {
        return await _customerService.SaveFavouriteDealer(favouriteDealerRequest);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet(StaticValues.FavouriteDealerPath)]
    [ProducesResponseType(typeof(DealerLocationProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DealerLocationProfileResponse?>> GetFavouriteDealer()
    {
        DealerLocationProfileResponse? favouriteDealerResponse = await _customerService.GetFavouriteDealer();
        if (favouriteDealerResponse == null)
        {
            return Ok();
        }

        return favouriteDealerResponse;
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet(StaticValues.DealerProfilesPath)]
    [ProducesResponseType(typeof(List<DealerProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<DealerProfileResponse>> GetDealerProfilesBySearchString(
        [FromQuery] string businessSearchString)
    {
        return await _customerService.GetDealerProfilesBySearchString(businessSearchString);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet(StaticValues.DealerLocationProfilesPath)]
    [ProducesResponseType(typeof(DealerLocationProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DealerLocationProfileResponse> GetDealerLocationProfile(int businessLocationId)
    {
        return await _dealerService.GetDealerLocationProfile(businessLocationId);
    }

    [HttpPost(StaticValues.CustomerProfilesByEmailInternalPath)]
    [ProducesResponseType(typeof(List<CustomerProfileByEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<CustomerProfileByEmailResponse>> GetCustomerProfiles(
        [FromBody] CustomerByEmailRequest customerByEmailRequest)
    {
        return await _customerService.GetCustomerProfiles(customerByEmailRequest);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpDelete(StaticValues.CustomerDeleteProfilePicturePath)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task DeleteCustomerProfilePicture([FromHeader(Name = StaticValues.EmailHeader)] string email)
    {
        await _customerService.DeleteCustomerProfilePicture(email);
    }

    [HttpPost(StaticValues.CustomerDeliveryInstructionPath)]
    [ProducesResponseType(typeof(DeliveryInstructionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DeliveryInstructionsResponse> SaveCustomerDeliveryInstructions(
        [FromBody] DeliveryInstructionsRequest deliveryInstructionsRequest)
    {
        return await _customerService.CreateOrUpdateDeliveryInstructions(deliveryInstructionsRequest);
    }

    [HttpGet(StaticValues.CustomerDeliveryInstructionPath)]
    [ProducesResponseType(typeof(DeliveryInstructionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<DeliveryInstructionsResponse> GetCustomerDeliveryInstructions()
    {
        return await _customerService.GetDeliveryInstructions();
    }

    [HttpPost(StaticValues.CustomerSuggestedDealerPath)]
    [ProducesResponseType(typeof(CustomerSuggestedDealerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<CustomerSuggestedDealerResponse> InviteNewDealer(
        [FromBody] CustomerSuggestedDealerRequest customerSuggestedDealerRequest)
    {
        return await _customerService.CreateCustomerSuggestedDealers(customerSuggestedDealerRequest);
    }

    [HttpPost(StaticValues.ProfilePhotoUploadPath)]
    [Authorize(UserRoleEnum.Customer)]
    [ProducesResponseType(typeof(ProfilePhotoUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ProfilePhotoUploadResponse>
        UploadCustomerProfilePhoto([FromForm] ProfilePhotoUploadRequest profilePhotoUploadRequest)
    {
        return await _customerService.UploadCustomerProfilePhoto(profilePhotoUploadRequest);
    }


    [Authorize(UserRoleEnum.Customer)]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task DeleteCustomer([FromHeader(Name = StaticValues.RequestHeaderForEmail)] string customerEmail)
    {
        await _customerService.DeleteCustomer(customerEmail);
    }

    [HttpPost(StaticValues.CustomersByEmailsInternalPath)]
    [ProducesResponseType(typeof(List<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<CustomerResponse>> GetCustomersByEmails([FromBody] List<string> emails)
    {
        return await _customerService.GetCustomers(emails);
    }

    [HttpGet(StaticValues.CustomerIdInternalPath)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<int> GetCustomerIdByEmail([FromQuery] [Required] string email)
    {
        return await _customerService.GetCustomerIdByEmail(email);
    }

    [HttpPost(StaticValues.CustomerFreeCallStatusInternalPath)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task UpdateFirstFreeCallAvailed([FromQuery] string email, [FromQuery] bool status)
    {
        await _customerService.UpdateFirstFreeCallAvailed(email, status);
    }

    [Authorize(UserRoleEnum.Customer)]
    [HttpGet(StaticValues.CustomerFreeCallStatusPath)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<bool> GetFirstFreeCallAvailed(
        [FromHeader(Name = StaticValues.RequestHeaderForEmail)]
        string email)
    {
        return await _customerService.GetFirstFreeCallAvailed(email);
    }
}
