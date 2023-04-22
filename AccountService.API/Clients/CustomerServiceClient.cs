using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.CustomerServiceClient;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Clients;

public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientUtility _httpClientUtility;

    public CustomerServiceClient(HttpClient httpClient, HttpClientUtility httpClientUtility)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (_httpClient.BaseAddress == null)
        {
            throw new NotFoundException(StaticValues.CustomerServiceBaseAddressNotFound,
                StaticValues.ErrorCustomerServiceBaseAddressNotFound);
        }

        _httpClientUtility = httpClientUtility;
    }

    public async Task<List<ReminderResponse>> CreateCustomerReminders(
        CreateCustomerReminderRequest createCustomerReminderRequest)
    {
        if (_httpClient.BaseAddress == null)
        {
            throw new NotFoundException(StaticValues.CustomerServiceBaseAddressNotFound,
                StaticValues.ErrorCustomerServiceBaseAddressNotFound);
        }

        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.CreateCustomerRemindersPath);
        HttpContent content = new StringContent(JsonConvert.SerializeObject(createCustomerReminderRequest),
            Encoding.UTF8,
            "application/json");
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(uriPath, content);
        return await _httpClientUtility.PrepareResponse<List<ReminderResponse>>(httpResponseMessage, uriPath);
    }
}
