using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.JobServiceClient;
using Microsoft.AspNetCore.WebUtilities;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Clients;

public class JobServiceClient : IJobServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientUtility _httpClientUtility;

    public JobServiceClient(HttpClient httpClient, HttpClientUtility httpClientUtility)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (_httpClient.BaseAddress == null)
        {
            throw new NotFoundException(StaticValues.CatalogServiceBaseAddressNotFound,
                StaticValues.ErrorJobServiceBaseAddressNotFound);
        }

        _httpClientUtility = httpClientUtility;
    }

    public async Task<List<WorkOrderResponse>> GetWorkOrdersByBusinessLocationIds(List<int> businessLocationIds)
    {
        IEnumerable<KeyValuePair<string, string>> queryParams = businessLocationIds.ConvertAll(locationId =>
            new KeyValuePair<string, string>(StaticValues.BusinessLocationIdsQueryParam, locationId.ToString()));
        string baseUrl = $"{_httpClient.BaseAddress}{StaticValues.InternalDealersWorkOrdersPath}";
        Uri uriPath = new(QueryHelpers.AddQueryString(baseUrl, queryParams!));
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uriPath);
        return await _httpClientUtility.PrepareResponse<List<WorkOrderResponse>>(httpResponseMessage, uriPath);
    }
}
