using System.Threading.Tasks;
using AccountService.API.Config;
using AccountService.API.Config.Interfaces;
using AccountService.API.Constants;
using AccountService.API.Dto.EmailParameters;
using AccountService.API.Dto.NotificationServiceClient;
using AccountService.API.Models;
using AccountService.API.Services.Interfaces;
using Newtonsoft.Json;

namespace AccountService.API.Services;

public class NotificationService : INotificationService
{
    private readonly AppConfig _appConfig;
    private readonly IServiceBusConfig _serviceBusConfig;

    public NotificationService(AppConfig appConfig, IServiceBusConfig serviceBusConfig)
    {
        _appConfig = appConfig;
        _serviceBusConfig = serviceBusConfig;
    }

    public async Task NotifyDstOnDealerSignUp(Dealer dealer)
    {
        DealerSignUpEmailParameters dealerSignUpEmailParameters = new()
        {
            RegistrationDate = dealer.UpdatedAt.ToShortDateString(),
            BusinessName = dealer.Business.Name,
            Email = dealer.Account.Email,
            Address = dealer.Business.Locations[0].Address
        };
        EmailRequestWithTemplateParameters emailRequestWithTemplateParameters = new()
        {
            RecipientAddress = _appConfig.DstEmailAddress,
            Subject = StaticValues.NewDealerSignUpSubject,
            SenderAddress = _appConfig.SenderEmailAddress,
            HtmlTemplateName = StaticValues.DSTDealerSignUpEmailTemplateName,
            HtmlTemplateParameters = dealerSignUpEmailParameters
        };
        string message = JsonConvert.SerializeObject(emailRequestWithTemplateParameters, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        await _serviceBusConfig.SendEmailMessageToQueue(message);
    }
}
