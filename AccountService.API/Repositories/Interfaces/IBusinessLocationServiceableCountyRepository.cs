using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.API.Models;

namespace AccountService.API.Repositories.Interfaces;

public interface IBusinessLocationServiceableCountyRepository
{
    public Task<List<BusinessLocationServiceableCounty>> FindByCounty(string countyName);
}
