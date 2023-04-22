using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface IBusinessRepository
{
    Task<Business> SaveBusiness(Business business);
}
