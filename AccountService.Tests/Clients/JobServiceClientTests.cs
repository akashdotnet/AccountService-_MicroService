using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountService.API.Clients;
using AccountService.API.Dto.JobServiceClient;
using AutoFixture;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;
using Xunit;

namespace AccountService.API.Tests.Clients;

public class JobServiceClientTests
{
    private readonly IFixture _fixture;

    public JobServiceClientTests()
    {
        _fixture = new Fixture();
    }

    [Fact(DisplayName =
        "JobServiceClient: GetWorkOrdersByBusinessLocationIds - Should be able to get work orders for the given business location ids.")]
    public async Task GetWorkOrdersByBusinessLocationIds_Success()
    {
        // arrange
        List<WorkOrderResponse> workOrdersMock = _fixture.Create<List<WorkOrderResponse>>();

        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.Accepted,
            Content = new StringContent(JsonConvert.SerializeObject(workOrdersMock))
        };
        JobServiceClient jobServiceClient = GetJobServiceClientObject(httpResponseMessage);

        // act
        List<WorkOrderResponse> result = await jobServiceClient.GetWorkOrdersByBusinessLocationIds(new List<int> {1});

        // assert
        Assert.NotNull(result);
        Assert.Equal(JsonConvert.SerializeObject(workOrdersMock), JsonConvert.SerializeObject(result));
    }

    [Theory(DisplayName =
        "JobServiceClient : GetWorkOrdersByBusinessLocationIds - Unable to get state and county on the basis of zip code: failure scenarios due to internal server error and service unavailable.")]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetWorkOrdersByBusinessLocationIds_Failure(HttpStatusCode httpStatusCode)
    {
        // arrange
        List<WorkOrderResponse> workOrdersMock = _fixture.Create<List<WorkOrderResponse>>();

        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode
        };
        JobServiceClient jobServiceClient = GetJobServiceClientObject(httpResponseMessage);

        // assert and act                                                                                                                     
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await jobServiceClient.GetWorkOrdersByBusinessLocationIds(new List<int> {1}));
    }

    [Fact(DisplayName = "JobServiceClient: Setup - Should throw NotFoundException when there is no base address.")]
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
            new JobServiceClient(httpClient, httpClientUtility));

        // assert
        Assert.Equal("The base address for job service is not found!", exception.Message);
    }

    private JobServiceClient GetJobServiceClientObject(HttpResponseMessage message)
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
        return new JobServiceClient(httpClient, httpClientUtility);
    }
}
