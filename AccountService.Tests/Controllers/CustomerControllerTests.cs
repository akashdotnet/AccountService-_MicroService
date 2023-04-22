using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Controllers;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AccountService.API.Tests.Controllers;

public class CustomerControllerTests
{
    private readonly CustomerController _customerController;
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<IDealerService> _dealerServiceMock;
    private readonly Mock<IExpertService> _expertServiceMock;
    private readonly IFixture _fixture;

    public CustomerControllerTests()
    {
        _fixture = new Fixture();

        _customerServiceMock = new Mock<ICustomerService>();
        _expertServiceMock = new Mock<IExpertService>();
        _dealerServiceMock = new Mock<IDealerService>();
        _customerController = new CustomerController(_customerServiceMock.Object, _expertServiceMock.Object,
            _dealerServiceMock.Object);
    }

    [Fact(DisplayName =
        "CustomerController: SaveFavouriteDealer - Should successfully set the favourite dealer for the customer.")]
    public async Task SaveFavouriteDealer_Success()
    {
        FavouriteDealerRequest favouriteDealerRequestMock = _fixture.Create<FavouriteDealerRequest>();
        DealerLocationProfileResponse dealerLocationProfileResponseMock =
            _fixture.Create<DealerLocationProfileResponse>();

        // arrange
        _customerServiceMock.Setup(x => x.SaveFavouriteDealer(favouriteDealerRequestMock))
            .ReturnsAsync(dealerLocationProfileResponseMock);

        //act
        DealerLocationProfileResponse
            result = await _customerController.SaveFavouriteDealer(favouriteDealerRequestMock);

        // assert
        Assert.IsType<DealerLocationProfileResponse>(result);
        Assert.Equal(dealerLocationProfileResponseMock, result);
    }

    [Fact(DisplayName =
        "CustomerController: GetFavouriteDealer - Should successfully get the favourite dealer of the customer.")]
    public async Task GetFavouriteDealer_Success()
    {
        DealerLocationProfileResponse favouriteDealerResponseMock =
            _fixture.Create<DealerLocationProfileResponse>();

        // arrange
        _customerServiceMock.Setup(x => x.GetFavouriteDealer())
            .ReturnsAsync(favouriteDealerResponseMock);

        //act
        ActionResult<DealerLocationProfileResponse?>
            result = await _customerController.GetFavouriteDealer();

        // assert
        Assert.NotNull(result.Value);
        Assert.IsType<DealerLocationProfileResponse?>(result.Value);
        Assert.Equal(favouriteDealerResponseMock, result.Value);
    }


    [Fact(DisplayName =
        "DealerController: GetDealerProfilesBySearchString - Should successfully get the list of all dealers whose business name starts with same value as businessSearchString input and which services the customer state.")]
    public async Task GetDealerProfilesByCustomerState_Success()
    {
        const string businessSearchStringMock = "test";
        List<DealerProfileResponse> dealerProfileResponsesMock = _fixture.Create<List<DealerProfileResponse>>();

        // arrange
        _customerServiceMock.Setup(x => x.GetDealerProfilesBySearchString(businessSearchStringMock))
            .ReturnsAsync(dealerProfileResponsesMock);

        //act
        List<DealerProfileResponse> result =
            await _customerController.GetDealerProfilesBySearchString(businessSearchStringMock);

        // assert
        Assert.IsType<List<DealerProfileResponse>>(result);
        Assert.Equal(dealerProfileResponsesMock, result);
        _customerServiceMock.Verify(x => x.GetDealerProfilesBySearchString(businessSearchStringMock), Times.Once);
    }

    [Fact(DisplayName =
        "DealerController: GetDealerLocationProfile - Should successfully get the business location and the related dealer based on business location id.")]
    public async Task GetDealerLocationProfile_Success()
    {
        const int businessLocationId = 1;
        DealerLocationProfileResponse dealerLocationProfileResponseMock =
            _fixture.Create<DealerLocationProfileResponse>();

        // arrange
        _dealerServiceMock.Setup(x => x.GetDealerLocationProfile(businessLocationId))
            .ReturnsAsync(dealerLocationProfileResponseMock);

        //act
        DealerLocationProfileResponse result = await _customerController.GetDealerLocationProfile(businessLocationId);

        // assert
        Assert.IsType<DealerLocationProfileResponse>(result);
        Assert.Equal(dealerLocationProfileResponseMock, result);
        _dealerServiceMock.Verify(x => x.GetDealerLocationProfile(businessLocationId), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerController: UploadCustomerProfilePhoto - Should successfully upload customer profile photo and return profile photo url.")]
    public async Task UploadCustomerProfilePhoto_Success()
    {
        //arrange
        ProfilePhotoUploadRequest profilePhotoUploadRequest =
            _fixture.Build<ProfilePhotoUploadRequest>().OmitAutoProperties().Create();
        ProfilePhotoUploadResponse profilePhotoUploadResponse = _fixture.Build<ProfilePhotoUploadResponse>().Create();
        _customerServiceMock.Setup(x => x.UploadCustomerProfilePhoto(profilePhotoUploadRequest))
            .ReturnsAsync(profilePhotoUploadResponse);

        //act
        ProfilePhotoUploadResponse response =
            await _customerController.UploadCustomerProfilePhoto(profilePhotoUploadRequest);

        //assert
        Assert.IsType<ProfilePhotoUploadResponse>(response);
        Assert.Equal(profilePhotoUploadResponse, response);
        _customerServiceMock.Verify(x => x.UploadCustomerProfilePhoto(profilePhotoUploadRequest), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerController: GetCustomerIdByEmail - Should successfully get the customer id for the given customer email.")]
    public async Task GetCustomerIdByEmail_Success()
    {
        // arrange
        const string customerEmail = "test@mail.com";
        const int customerId = 1;
        _customerServiceMock.Setup(x => x.GetCustomerIdByEmail(customerEmail))
            .ReturnsAsync(customerId);

        //act
        int result = await _customerController.GetCustomerIdByEmail(customerEmail);

        // assert
        Assert.IsType<int>(result);
        Assert.Equal(customerId, result);
        _customerServiceMock.Verify(x => x.GetCustomerIdByEmail(customerEmail), Times.Once);
    }

    [Fact(DisplayName =
        "CustomerController: UpdateCustomerNotificationPreference - Should successfully update a customer's notification preferences.")]
    public async Task UpdateCustomerNotificationPreference_Success()
    {
        // arrange
        NotificationPreferenceRequest notificationPreferenceRequest = _fixture.Create<NotificationPreferenceRequest>();
        _customerServiceMock.Setup(x =>
            x.UpdateCustomerNotificationPreferences(notificationPreferenceRequest));

        //act
        await _customerController.UpdateCustomerNotificationPreferences(
            notificationPreferenceRequest);

        // assert
        _customerServiceMock.Verify(
            x => x.UpdateCustomerNotificationPreferences(notificationPreferenceRequest),
            Times.Once);
    }

    [Fact(DisplayName =
        "CustomerController: GetNotificationPreferences - Should successfully get a customer's notification preferences.")]
    public async Task GetNotificationPreferences_Success()
    {
        // arrange
        _customerServiceMock.Setup(x =>
            x.GetNotificationPreferences());

        //act
        await _customerController.GetNotificationPreferences();

        // assert
        _customerServiceMock.Verify(
            x => x.GetNotificationPreferences(),
            Times.Once);
    }
}
