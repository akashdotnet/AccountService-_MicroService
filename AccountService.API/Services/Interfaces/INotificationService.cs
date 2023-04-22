using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Services.Interfaces;

public interface INotificationService
{
    Task NotifyDstOnDealerSignUp(Dealer dealer);
}
