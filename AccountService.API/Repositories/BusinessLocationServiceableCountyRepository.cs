using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountService.API.Data;
using AccountService.API.Models;
using AccountService.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountService.API.Repositories;

public class BusinessLocationServiceableCountyRepository : IBusinessLocationServiceableCountyRepository
{
    private readonly AccountServiceDbContext _accountServiceDbContext;

    public BusinessLocationServiceableCountyRepository(AccountServiceDbContext accountServiceDbContext)
    {
        _accountServiceDbContext = accountServiceDbContext;
    }

    public async Task<List<BusinessLocationServiceableCounty>> FindByCounty(string countyName)
    {
        return await _accountServiceDbContext.BusinessLocationServiceableCounties
            .Include("BusinessLocation")
            .Where(x => x.ServiceableCounty.Equals(countyName))
            .ToListAsync();
    }
}
