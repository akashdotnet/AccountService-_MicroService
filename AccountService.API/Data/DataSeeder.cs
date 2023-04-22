using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Data;

public class DataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public DataSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task Seed()
    {
        await SeedCustomerRole();
        await SeedDealerRole();
        await SeedExpertRole();
    }

    private async Task SeedCustomerRole()
    {
        if (!await _roleManager.RoleExistsAsync(UserRoleEnum.Customer.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoleEnum.Customer.ToString()));
        }
    }

    private async Task SeedDealerRole()
    {
        if (!await _roleManager.RoleExistsAsync(UserRoleEnum.Dealer.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoleEnum.Dealer.ToString()));
        }
    }

    private async Task SeedExpertRole()
    {
        if (!await _roleManager.RoleExistsAsync(UserRoleEnum.Expert.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoleEnum.Expert.ToString()));
        }
    }
}
