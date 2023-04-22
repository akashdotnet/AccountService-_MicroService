using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.PaymentServiceClient;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Clients;

public class PaymentServiceClient : IPaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientUtility _httpClientUtility;

    public PaymentServiceClient(HttpClient httpClient, HttpClientUtility httpClientUtility)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (_httpClient.BaseAddress == null)
        {
            throw new NotFoundException(StaticValues.PaymentServiceBaseAddressNotFound,
                StaticValues.ErrorPaymentServiceBaseAddressNotFound);
        }

        _httpClientUtility = httpClientUtility;
    }

    public async Task<CreateCustomerForPaymentResponse> CreateCustomer(
        CreateCustomerForPaymentRequest createCustomerRequest)
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.CustomerPath);
        HttpContent content = new StringContent(JsonConvert.SerializeObject(createCustomerRequest), Encoding.UTF8,
            "application/json");
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(uriPath, content);
        return await _httpClientUtility.PrepareResponse<CreateCustomerForPaymentResponse>(httpResponseMessage, uriPath);
    }

    public async Task CreateDealerBankAccount(string email, string userType)
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.DealerBankAccountPath);
        _httpClient.DefaultRequestHeaders.Add(StaticValues.EmailHeader, email);
        _httpClient.DefaultRequestHeaders.Add(StaticValues.UserTypeHeader, userType);
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(uriPath, new StringContent(string.Empty));
        await _httpClientUtility.PrepareResponse(httpResponseMessage, uriPath);
    }
}
