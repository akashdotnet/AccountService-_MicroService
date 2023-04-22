using System.Threading.Tasks;
using AccountService.API.Controllers;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using AutoMapper;
using Moq;
using Xunit;

namespace AccountService.API.Tests.Controllers;

public class DealerControllerTests
{
    private readonly DealerController _dealerController;
    private readonly Mock<IDealerService> _dealerServiceMock;
    private readonly IFixture _fixture;
    private readonly Mock<IMapper> _mapperMock;

    public DealerControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dealerServiceMock = new Mock<IDealerService>();
        _mapperMock = new Mock<IMapper>();
        _dealerController = new DealerController(_dealerServiceMock.Object, _mapperMock.Object);
    }

    [Fact(DisplayName = "DealerController: UpdateDealer - Should successfully update dealer.")]
    public async Task UpdateDealer_Success()
    {
        UpdateDealerRequest updateDealerRequestMock = _fixture.Create<UpdateDealerRequest>();
        Dealer dealerMock = _fixture.Create<Dealer>();
        DealerResponse dealerResponseMock = _fixture.Create<DealerResponse>();
        const string userType = "1";

        // arrange
        _dealerServiceMock.Setup(x => x.UpdateDealer(updateDealerRequestMock, userType)).ReturnsAsync(dealerMock);
        _mapperMock.Setup(x => x.Map<DealerResponse>(dealerMock)).Returns(dealerResponseMock);

        //act
        DealerResponse result = await _dealerController.UpdateDealer(updateDealerRequestMock, userType);

        // assert
        Assert.IsType<DealerResponse>(result);
        Assert.Equal(dealerResponseMock, result);
    }

    [Fact(DisplayName = "DealerController: GetDealer - Should successfully get dealer.")]
    public async Task GetDealer_Success()
    {
        DealerResponse dealerResponseMock = _fixture.Create<DealerResponse>();

        // arrange
        _dealerServiceMock.Setup(x => x.GetDealerWithBusinessLogo()).ReturnsAsync(dealerResponseMock);

        //act
        DealerResponse result = await _dealerController.GetDealer();

        // assert
        Assert.IsType<DealerResponse>(result);
        Assert.Equal(dealerResponseMock, result);
    }

    [Fact(DisplayName =
        "DealerController: UpdateDealerTermsAndConditions - Should successfully update dealer's terms and conditions.")]
    public async Task UpdateDealerTermsAndConditions_Success()
    {
        DealerTermsAndConditionsRequest dealerTermsAndConditionsRequest = _fixture
            .Build<DealerTermsAndConditionsRequest>()
            .With(x => x.TermsAndConditionsAccepted, true)
            .Create();
        DealerTermsAndConditionsResponse dealerTermsAndConditionsResponse = _fixture
            .Build<DealerTermsAndConditionsResponse>()
            .With(x => x.TermsAndConditionsAccepted, true)
            .Create();

        // arrange
        _dealerServiceMock.Setup(x => x.UpdateDealerTermsAndConditions(dealerTermsAndConditionsRequest))
            .ReturnsAsync(dealerTermsAndConditionsResponse);

        //act
        DealerTermsAndConditionsResponse result =
            await _dealerController.UpdateDealerTermsAndConditions(dealerTermsAndConditionsRequest);

        // assert
        Assert.IsType<DealerTermsAndConditionsResponse>(result);
        Assert.Equal(dealerTermsAndConditionsResponse.TermsAndConditionsAccepted, result.TermsAndConditionsAccepted);
    }
}
