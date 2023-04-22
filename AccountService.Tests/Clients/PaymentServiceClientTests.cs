using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountService.API.Clients;
using AccountService.API.Dto.PaymentServiceClient;
using AutoFixture;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;
using Xunit;

namespace AccountService.API.Tests.Clients;

public class PaymentServiceClientTests
{
    private readonly IFixture _fixture;

    public PaymentServiceClientTests()
    {
        _fixture = new Fixture();
    }

    private PaymentServiceClient GetPaymentServiceClientObject(HttpResponseMessage message)
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
        return new PaymentServiceClient(httpClient, httpClientUtility);
    }

    [Fact(DisplayName = "PaymentServiceClient: Setup - Should throw NotFoundException when there is no base address.")]
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
            new PaymentServiceClient(httpClient, httpClientUtility));

        // assert
        Assert.Equal("The base address for payment service is not found!", exception.Message);
    }

    [Fact(DisplayName = "PaymentServiceClient: CreateCustomer - Should create a customer account in Stripe.")]
    public async Task CreateCustomer_Success()
    {
        // arrange
        CreateCustomerForPaymentRequest request = _fixture.Create<CreateCustomerForPaymentRequest>();

        CreateCustomerForPaymentResponse response = _fixture.Build<CreateCustomerForPaymentResponse>()
            .With(x => x.StripeCustomerId, "stripe-customer-id")
            .Create();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = HttpStatusCode.Accepted,
            Content = new StringContent(JsonConvert.SerializeObject(response))
        };
        PaymentServiceClient client = GetPaymentServiceClientObject(httpResponseMessage);

        // act
        CreateCustomerForPaymentResponse result = await client.CreateCustomer(request);

        // assert
        Assert.NotNull(result);
        Assert.Equal("stripe-customer-id", result.StripeCustomerId);
    }

    [Theory(DisplayName =
        "PaymentServiceClient : CreateCustomer - Should throw HttpRequestException in failure scenarios such as InternalServerError, ServiceUnavailable and BadRequest.")]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task CreateCustomer_Failure(HttpStatusCode httpStatusCode)
    {
        // arrange
        CreateCustomerForPaymentRequest request = _fixture.Create<CreateCustomerForPaymentRequest>();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode
        };
        PaymentServiceClient client = GetPaymentServiceClientObject(httpResponseMessage);

        // assert and act                                                                                                                     
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.CreateCustomer(request));
    }
}
