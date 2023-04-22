using System.Threading.Tasks;
using AccountService.API.Dto.VideoCallServiceClient;

namespace AccountService.API.Clients.Interfaces;

public interface IVideoCallServiceClient
{
    Task<AgentResponse> CreateAgent(CreateAgentRequest createAgentRequest);
}
