using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountService.API.Clients;
using AccountService.API.Dto.CatalogServiceClient;
using AutoFixture;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Utils;
using Xunit;

namespace AccountService.API.Tests.Clients;

public class CatalogServiceClientTests
{
    private readonly IFixture _fixture;

    public CatalogServiceClientTests()
    {
        _fixture = new Fixture();
    }

    [Theory(DisplayName = "CatalogServiceClient : GetPoolDetailLookups - Should be able to get pool detail lookups.")]
    [InlineData(HttpStatusCode.OK)]
    public async Task GetPoolCategories_Success(HttpStatusCode httpStatusCode)
    {
        // arrange
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(GetPoolDetailLookupsObject()))
        };
        CatalogServiceClient catalogServiceClient = GetCatalogServiceClientObject(httpResponseMessage);

        // act
        PoolDetailLookups result = await catalogServiceClient.GetPoolDetailLookups();

        // assert
        Assert.NotNull(result);
        Assert.Contains(result.PoolTypes, x => x.Code == "lap_pool");
        Assert.Contains(result.PoolMaterials, x => x.Code == "fibreglass");
        Assert.Contains(result.HotTubTypes, x => x.Code == "none");
        Assert.Contains(result.PoolSizes, x => x.Code == "medium");
        Assert.Contains(result.SanitationMethods, x => x.Code == "water_pumps");
        Assert.Contains(result.PoolSeasons, x => x.Code == "open_closed_seasons");
    }

    [Theory(DisplayName =
        "CatalogServiceClient : GetPoolDetailLookups - Unable to get pool detail lookups : failure scenarios.")]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetPoolCategories_Failure(HttpStatusCode httpStatusCode)
    {
        // arrange
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(GetPoolDetailLookupsObject()))
        };
        CatalogServiceClient catalogServiceClient = GetCatalogServiceClientObject(httpResponseMessage);

        // assert and act                                                                                                                     
        await Assert.ThrowsAsync<HttpRequestException>(async () => await catalogServiceClient.GetPoolDetailLookups());
    }

    [Theory(DisplayName = "CatalogServiceClient : GetStates - Should be able to get list of states.")]
    [InlineData(HttpStatusCode.OK)]
    public async Task GetStates_Success(HttpStatusCode httpStatusCode)
    {
        // arrange
        List<StateResponse> expectedStates = GetStatesObject();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(expectedStates))
        };
        CatalogServiceClient catalogServiceClient = GetCatalogServiceClientObject(httpResponseMessage);

        // act
        List<StateResponse> result = await catalogServiceClient.GetStates(true);

        // assert
        Assert.NotNull(result);
        Assert.Equal(JsonConvert.SerializeObject(expectedStates), JsonConvert.SerializeObject(result));
    }

    [Theory(DisplayName = "CatalogServiceClient : GetStates - Unable to get states : failure scenarios.")]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetStates_Failure(HttpStatusCode httpStatusCode)
    {
        // arrange
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode
        };
        CatalogServiceClient catalogServiceClient = GetCatalogServiceClientObject(httpResponseMessage);

        // assert and act                                                                                                                     
        await Assert.ThrowsAsync<HttpRequestException>(async () => await catalogServiceClient.GetStates(true));
    }

    [Theory(DisplayName =
        "CatalogServiceClient : GetStateAndCountyByZipCode - Should be able to get state and county on the basis of zip code.")]
    [InlineData(HttpStatusCode.OK)]
    public async Task GetStatesAndCountyByZipCode_Success(HttpStatusCode httpStatusCode)
    {
        // arrange
        const string validState = "California";
        const string countyName = "Jefferson";

        StateByZipCodeResponse stateByZipCodeDataCalifornia = _fixture.Build<StateByZipCodeResponse>()
            .With(x => x.Name, validState)
            .With(x => x.County, new CountyByZipCodeResponse {Name = countyName}).Create();

        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(stateByZipCodeDataCalifornia))
        };
        CatalogServiceClient catalogServiceClient = GetCatalogServiceClientObject(httpResponseMessage);

        // act
        StateByZipCodeResponse result = await catalogServiceClient.GetStateAndCountyByZipCode(It.IsAny<string>());

        // assert
        Assert.NotNull(result);
        Assert.Equal(JsonConvert.SerializeObject(stateByZipCodeDataCalifornia), JsonConvert.SerializeObject(result));
    }

    [Theory(DisplayName =
        "CatalogServiceClient : GetStateAndCountyByZipCode - Unable to get state and county on the basis of zip code: failure scenarios due to internal server error and service unavailable.")]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetStatesAndCountyByZipCode_Failure(HttpStatusCode httpStatusCode)
    {
        // arrange
        // arrange
        const string validState = "California";
        const string countyName = "Jefferson";
        CountyResponse countyData = _fixture.Build<CountyResponse>()
            .With(x => x.Name, countyName)
            .Create();

        StateResponse stateData = _fixture.Build<StateResponse>()
            .With(x => x.Name, validState)
            .With(x => x.Counties, new List<CountyResponse> {countyData}).Create();
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonConvert.SerializeObject(stateData))
        };
        CatalogServiceClient catalogServiceClient = GetCatalogServiceClientObject(httpResponseMessage);

        // assert and act                                                                                                                     
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await catalogServiceClient.GetStateAndCountyByZipCode(It.IsAny<string>()));
    }

    private PoolDetailLookups GetPoolDetailLookupsObject()
    {
        PoolDetailLookups poolDetailLookups = new();
        PoolSizeResponse poolSizeCode = _fixture.Build<PoolSizeResponse>()
            .With(x => x.Code, "medium").Create();
        poolDetailLookups.PoolSizes.Add(poolSizeCode);

        PoolMaterialResponse poolMaterialCode = _fixture.Build<PoolMaterialResponse>()
            .With(x => x.Code, "fibreglass").Create();
        poolDetailLookups.PoolMaterials.Add(poolMaterialCode);

        HotTubTypeResponse hotTubTypeCode = _fixture.Build<HotTubTypeResponse>()
            .With(x => x.Code, "none").Create();
        poolDetailLookups.HotTubTypes.Add(hotTubTypeCode);

        PoolTypeResponse poolTypeCode = _fixture.Build<PoolTypeResponse>()
            .With(x => x.Code, "lap_pool").Create();
        poolDetailLookups.PoolTypes.Add(poolTypeCode);

        SanitationMethodResponse sanitationMethodCode = _fixture.Build<SanitationMethodResponse>()
            .With(x => x.Code, "water_pumps").Create();
        poolDetailLookups.SanitationMethods.Add(sanitationMethodCode);

        PoolSeasonResponse poolSeasonCode = _fixture.Build<PoolSeasonResponse>()
            .With(x => x.Code, "open_closed_seasons").Create();
        poolDetailLookups.PoolSeasons.Add(poolSeasonCode);

        return poolDetailLookups;
    }

    private CatalogServiceClient GetCatalogServiceClientObject(HttpResponseMessage message)
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
            BaseAddress = new Uri("http://test.com/")
        };
        HttpClientUtility httpClientUtility = new();
        return new CatalogServiceClient(httpClient, httpClientUtility);
    }

    private List<StateResponse> GetStatesObject()
    {
        return _fixture.Create<List<StateResponse>>();
    }

    private CatalogServiceClient GetCatalogServiceClientObjectWithNullBaseAddress(HttpResponseMessage message)
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
        HttpClient httpClient = new(handlerMock.Object);
        HttpClientUtility httpClientUtility = new();
        return new CatalogServiceClient(httpClient, httpClientUtility);
    }
}
