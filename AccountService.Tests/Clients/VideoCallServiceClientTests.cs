using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountService.API.Clients;
using AccountService.API.Dto.VideoCallServiceClient;
using AutoFixture;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;
using Xunit;

namespace AccountService.API.Tests.Clients;

public class VideoCallServiceClientTests
{
    private readonly IFixture _fixture;

    public VideoCallServiceClientTests()
    {
        _fixture = new Fixture();
    }

    [Theory(DisplayName =
        "VideoCallServiceClient: CreateAgent - Should return user info from salesforce such as email, first name and last name.")]
    [InlineData(HttpStatusCode.OK)]
    public async Task CreateAgent_Success(HttpStatusCode httpStatusCode)
    {
        // arrange
        CreateAgentRequest createAgentRequestMock = _fixture.Create<CreateAgentRequest>();
        AgentResponse agentResponseMock = _fixture.Create<AgentResponse>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(agentResponseMock))
        };
        VideoCallServiceClient client = GetVideoCallServiceClientObject(httpResponseMessage);

        // act
        AgentResponse result = await client.CreateAgent(createAgentRequestMock);

        // assert
        Assert.Equal(agentResponseMock.Id, result.Id);
    }

    [Theory(DisplayName = "VideoCallServiceClient: CreateAgent - Should return bad request error object on failure.")]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task GetToken_Failure(HttpStatusCode httpStatusCode)
    {
        // arrange
        CreateAgentRequest createAgentRequestMock = _fixture.Create<CreateAgentRequest>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(_fixture
                .Build<ErrorResponse>()
                .With(x => x.Message, "Something Broke")
                .With(x => x.ErrorResponseType, "type")
                .Create()))
        };
        VideoCallServiceClient client = GetVideoCallServiceClientObject(httpResponseMessage);

        //act and assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.CreateAgent(createAgentRequestMock));
    }


    [Fact(DisplayName =
        "VideoCallServiceClient: Setup - Should throw NotFoundException when there is no base address.")]
    public void ClientSetup_Failure()
    {
        // arrange
        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Loose);
        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = null
        };
        HttpClientUtility httpClientUtility = new();

        // assert
        NotFoundException exception = Assert.ThrowsAny<NotFoundException>(() =>
            new VideoCallServiceClient(httpClient, httpClientUtility));

        // assert
        Assert.Equal("The base address for video call service is not found!", exception.Message);
    }

    private static VideoCallServiceClient GetVideoCallServiceClientObject(HttpResponseMessage message)
    {
        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Loose);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // prepare the expected response of the mocked http call
            .ReturnsAsync(message);

        // use real http client with mocked handler here
        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new Uri("https://test.com/")
        };
        HttpClientUtility httpClientUtility = new();
        return new VideoCallServiceClient(httpClient, httpClientUtility);
    }
}
