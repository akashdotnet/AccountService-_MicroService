using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Config;
using AccountService.API.Constants;
using AccountService.API.Dto.CatalogServiceClient;
using AccountService.API.Dto.Request;
using AccountService.API.Dto.Response;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AutoFixture;
using AutoFixture.DataAnnotations;
using AutoMapper;
using Moq;
using PodCommonsLibrary.Core.Exceptions;
using Xunit;

namespace AccountService.API.Tests.Services;

public class ExpertServiceTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly Mock<ICatalogServiceClient> _catalogServiceClientMock;
    private readonly Mock<IExpertRepository> _expertRepositoryMock;
    private readonly ExpertService _expertService;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    private readonly Mock<IVideoCallServiceClient> _videoCallServiceClientMock;


    public ExpertServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new RegularExpressionGenerator());
        _fixture.Customizations.Add(new RegularExpressionAttributeRelay());
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accountServiceMock = new Mock<IAccountService>();
        _catalogServiceClientMock = new Mock<ICatalogServiceClient>();
        _expertRepositoryMock = new Mock<IExpertRepository>();
        _videoCallServiceClientMock = new Mock<IVideoCallServiceClient>();
        _mapper = new MapperConfiguration(c =>
            c.AddProfile<MappingProfile>()).CreateMapper();
        _expertService = new ExpertService(_expertRepositoryMock.Object, _mapper,
            _catalogServiceClientMock.Object, _accountServiceMock.Object, _videoCallServiceClientMock.Object);
    }

    [Fact(DisplayName =
        "ExpertService: GetAllExpertProfiles - Should get profiles for all onboarded experts with their profile photo urls.")]
    public async Task GetAllExpertProfiles_SuccessMultipleExperts()
    {
        Expert expert1Mock = _fixture.Build<Expert>()
            .With(x => x.ProfilePhotoBlobId, 1)
            .Create();
        Expert expert2Mock = _fixture.Build<Expert>()
            .With(x => x.ProfilePhotoBlobId, 2)
            .Create();
        List<Expert> expertsMock = new()
        {
            expert1Mock,
            expert2Mock
        };

        BlobResponse blobResponseMock1 = _fixture.Build<BlobResponse>()
            .With(x => x.BlobUrl, "https://www.blob-url-1.com")
            .Create();
        BlobResponse blobResponseMock2 = _fixture.Build<BlobResponse>()
            .With(x => x.BlobUrl, "https://www.blob-url-2.com")
            .Create();

        ExpertResponse expertResponse1 = _fixture.Create<ExpertResponse>();
        ExpertResponse expertResponse2 = _fixture.Create<ExpertResponse>();

        _expertRepositoryMock.Setup(expertRepository => expertRepository
                .GetExperts())
            .ReturnsAsync(expertsMock);
        _catalogServiceClientMock.Setup(catalogService => catalogService
                .GetBlobUrl(1))
            .ReturnsAsync(blobResponseMock1);
        _catalogServiceClientMock.Setup(catalogService => catalogService
                .GetBlobUrl(2))
            .ReturnsAsync(blobResponseMock2);

        List<ExpertResponse> responses = await _expertService.GetAllExpertProfiles();
        Assert.Equal("https://www.blob-url-1.com", responses[0].ProfilePhotoUrl);
        Assert.Equal("https://www.blob-url-2.com", responses[1].ProfilePhotoUrl);
    }

    [Fact(DisplayName =
        "ExpertService: GetAllExpertProfiles - Should return an expert with profilePhotoUrl as null when no blob id is present.")]
    public async Task GetAllExpertProfiles_SuccessEmptyBlobId()
    {
        Expert expertMock = new()
        {
            ProfilePhotoBlobId = null
        };
        List<Expert> expertsMock = new()
        {
            expertMock
        };
        ExpertResponse expertResponse1 = _fixture.Create<ExpertResponse>();

        _expertRepositoryMock.Setup(expertRepository => expertRepository
                .GetExperts())
            .ReturnsAsync(expertsMock);

        List<ExpertResponse> responses = await _expertService.GetAllExpertProfiles();
        Assert.Null(responses[0].ProfilePhotoUrl);
    }

    [Fact(DisplayName =
        "ExpertService: GetExpertProfiles - Should be able to get expert profiles for request expert email ids.")]
    public async Task GetExpertProfiles_ByEmails_Success()
    {
        // arrange
        const string expert1Email = "expert1@mail.com";
        const string expert2Email = "expert2@mail.com";
        Expert expert1Mock = _fixture.Build<Expert>()
            .With(x => x.Account, _fixture.Build<Account>().With(x => x.Email, expert1Email).Create())
            .Create();
        Expert expert2Mock = _fixture.Build<Expert>()
            .With(x => x.Account, _fixture.Build<Account>().With(x => x.Email, expert2Email).Create())
            .Create();
        List<Expert> expertsMock = new()
        {
            expert1Mock,
            expert2Mock
        };

        List<string> emailIds = new()
        {
            expert1Email, expert2Email
        };
        ExpertByEmailRequest expertByEmailRequest = _fixture.Build<ExpertByEmailRequest>()
            .With(x => x.Emails, emailIds).Create();
        _expertRepositoryMock.Setup(x => x.GetExpertsByEmailIds(emailIds))
            .ReturnsAsync(expertsMock);

        // act
        List<ExpertResponse> results = await _expertService.GetExpertProfiles(expertByEmailRequest);

        // assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Contains(results, x => x.Email == expert1Email);
        Assert.Contains(results, x => x.Email == expert2Email);
    }

    [Fact(DisplayName =
        "ExpertService: GetExpertProfiles - Should throw an error if expert doesn't exist for request expert email id.")]
    public async Task GetExpertProfiles_ByEmails_ThrowsNotFoundError()
    {
        // arrange
        const string expert1Email = "expert1@mail.com";
        const string expert2Email = "expert2@mail.com";

        List<string> emailIds = new()
        {
            expert1Email, expert2Email
        };
        ExpertByEmailRequest expertByEmailRequest = _fixture.Build<ExpertByEmailRequest>()
            .With(x => x.Emails, emailIds).Create();
        _expertRepositoryMock.Setup(x => x.GetExpertsByEmailIds(emailIds))
            .ReturnsAsync(new List<Expert>());

        // act and assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _expertService.GetExpertProfiles(expertByEmailRequest));
        Assert.Equal(StaticValues.ExpertNotFound, exception.ErrorResponseType);
        Assert.Equal(StaticValues.ExpertNotFoundError, exception.Message);
    }
}
