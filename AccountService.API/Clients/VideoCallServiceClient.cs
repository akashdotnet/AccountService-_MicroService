using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.VideoCallServiceClient;
using Newtonsoft.Json;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Clients;

public class VideoCallServiceClient : IVideoCallServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientUtility _httpClientUtility;

    public VideoCallServiceClient(HttpClient httpClient, HttpClientUtility httpClientUtility)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (_httpClient.BaseAddress == null)
        {
            throw new NotFoundException(StaticValues.VideoCallServiceBaseAddressNotFound,
                StaticValues.ErrorVideoCallServiceBaseAddressNotFound);
        }

        _httpClientUtility = httpClientUtility;
    }

    public async Task<AgentResponse> CreateAgent(CreateAgentRequest createAgentRequest)
    {
        Uri uriPath = new(_httpClient.BaseAddress, StaticValues.AgentsPath);
        HttpContent content = new StringContent(JsonConvert.SerializeObject(createAgentRequest), Encoding.UTF8,
            "application/json");
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(uriPath, content);
        return await _httpClientUtility.PrepareResponse<AgentResponse>(httpResponseMessage, uriPath);
    }
}
