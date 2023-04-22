using System.Threading.Tasks;

namespace AccountService.API.Config.Interfaces;

public interface IServiceBusConfig
{
    Task SendEmailMessageToQueue(string message);
    Task SendDeleteAccountMessageToTopic(string message);
}
