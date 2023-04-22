using System.Threading.Tasks;
using AccountService.API.Config.Interfaces;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace AccountService.API.Config;

public class ServiceBusConfig : IServiceBusConfig
{
    private readonly string _deleteAccountTopicName;
    private readonly string _emailQueueName;
    private readonly ServiceBusClient _serviceBusClient;

    public ServiceBusConfig(IConfiguration configuration)
    {
        _serviceBusClient = new ServiceBusClient(configuration["ServiceBusConnectionString"]);
        _emailQueueName = configuration["ServiceBusEmailQueue"];
        _deleteAccountTopicName = configuration["ServiceBusDeleteAccountTopic"];
    }

    public async Task SendEmailMessageToQueue(string message)
    {
        await SendMessageToQueueOrTopic(message, _emailQueueName);
    }

    public async Task SendDeleteAccountMessageToTopic(string message)
    {
        await SendMessageToQueueOrTopic(message, _deleteAccountTopicName);
    }


    private async Task SendMessageToQueueOrTopic(string message, string queueOrTopicName)
    {
        ServiceBusSender sender = _serviceBusClient.CreateSender(queueOrTopicName);
        ServiceBusMessage serviceBusMessage = new(message);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}
