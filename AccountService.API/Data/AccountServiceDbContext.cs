using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AccountService.API.Constants;
using AccountService.API.Models;
using AccountService.API.Models.Audits;
using Audit.EntityFramework;
using Audit.EntityFramework.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PodCommonsLibrary.Core.Utils;

namespace AccountService.API.Data;

public class AccountServiceDbContext : AuditIdentityDbContext
{
    // Have a static instance of an EF DataProvider
    private static readonly Dictionary<Type, Type> AuditTypeDictionary = new()
    {
        {typeof(Account), typeof(AuditAccount)},
        {typeof(Address), typeof(AuditAddress)},
        {typeof(Customer), typeof(AuditCustomer)},
        {typeof(CustomerAddress), typeof(AuditCustomerAddress)},
        {typeof(CustomerDeliveryInstruction), typeof(AuditCustomerDeliveryInstruction)},
        {typeof(Dealer), typeof(AuditDealer)},
        {typeof(Expert), typeof(AuditExpert)},
        {typeof(ExpertLanguage), typeof(AuditExpertLanguage)},
        {typeof(ExpertSkill), typeof(AuditExpertSkill)},
        {typeof(Business), typeof(AuditBusiness)},
        {typeof(BusinessLocation), typeof(AuditBusinessLocation)},
        {typeof(BusinessBrand), typeof(AuditBusinessBrand)},
        {typeof(BusinessLocationServiceableCounty), typeof(AuditBusinessLocationServiceableCounty)},
        {typeof(BusinessJobCategory), typeof(AuditBusinessJobCategory)},
        {typeof(CustomerFavouriteDealerMapping), typeof(AuditCustomerFavouriteDealerMapping)},
        {typeof(CustomerSuggestedDealer), typeof(AuditCustomerSuggestedDealer)}
    };

    public AccountServiceDbContext()
    {
    }

    public AccountServiceDbContext(DbContextOptions<AccountServiceDbContext> options,
        IHttpContextAccessor httpContextAccessor) :
        base(options)
    {
        (string? userEmail, _) =
            AppUserIdentity.GetLoggedInUserClaims(httpContextAccessor.HttpContext?.Request.Headers);

        EntityFrameworkDataProvider efDataProvider = new()
        {
            AuditTypeMapper = (t, _) => !AuditTypeDictionary.ContainsKey(t) ? null : AuditTypeDictionary[t],
            AuditEntityAction = (_, entry, auditEntity) =>
            {
                dynamic? a = auditEntity;
                a.AuditDate = DateTime.UtcNow;
                a.AuditAction = entry.Action;
                a.UserName = userEmail ?? StaticValues.UnknownUserName;
                return Task.FromResult(true);
            }
        };
        AuditDataProvider = efDataProvider;
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Address> Address { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerDeliveryInstruction> CustomerDeliveryInstructions { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Dealer> Dealers { get; set; }
    public DbSet<Expert> Experts { get; set; }
    public DbSet<ExpertLanguage> ExpertLanguages { get; set; }
    public DbSet<ExpertSkill> ExpertSkills { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessLocation> BusinessLocations { get; set; }
    public DbSet<BusinessBrand> BusinessBrands { get; set; }
    public DbSet<BusinessLocationServiceableCounty> BusinessLocationServiceableCounties { get; set; }
    public DbSet<BusinessJobCategory> BusinessJobCategories { get; set; }
    public DbSet<CustomerFavouriteDealerMapping> CustomerFavouriteDealerMappings { get; set; }
    public DbSet<CustomerSuggestedDealer> CustomerSuggestedDealers { get; set; }
    public DbSet<AuditAccount> AuditAccounts { get; set; }
    public DbSet<AuditAddress> AuditAddresses { get; set; }
    public DbSet<AuditCustomer> AuditCustomers { get; set; }
    public DbSet<AuditCustomerDeliveryInstruction> AuditCustomerDeliveryInstructions { get; set; }
    public DbSet<AuditCustomerAddress> AuditCustomerAddresses { get; set; }
    public DbSet<AuditDealer> AuditDealers { get; set; }
    public DbSet<AuditExpert> AuditExperts { get; set; }
    public DbSet<AuditBusinessBrand> AuditBusinessBrands { get; set; }
    public DbSet<AuditExpertLanguage> AuditExpertLanguages { get; set; }
    public DbSet<AuditExpertSkill> AuditExpertSkills { get; set; }
    public DbSet<AuditBusiness> AuditBusinesses { get; set; }
    public DbSet<AuditBusinessLocation> AuditBusinessLocations { get; set; }
    public DbSet<AuditBusinessLocationServiceableCounty> AuditBusinessLocationServiceableCounties { get; set; }
    public DbSet<AuditBusinessJobCategory> AuditBusinessJobCategories { get; set; }
    public DbSet<ApiLogEntry> ApiLogEntries { get; set; }
    public DbSet<AuditCustomerFavouriteDealerMapping> AuditCustomerFavouriteDealerMappings { get; set; }
    public DbSet<AuditCustomerSuggestedDealer> AuditCustomerSuggestedDealers { get; set; }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private void AddTimestamps()
    {
        foreach (EntityEntry entity in ChangeTracker.Entries())
        {
            bool hasChanged = entity.State is EntityState.Added or EntityState.Modified;
            if (hasChanged)
            {
                BaseModel? baseModel = entity.Entity as BaseModel;
                if (baseModel != null)
                {
                    DateTime now = DateTime.UtcNow;
                    if (entity.State == EntityState.Added)
                    {
                        baseModel.CreatedAt = now;
                    }

                    baseModel.UpdatedAt = now;
                }
            }
        }
    }
}
