using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AccountService.API.Clients;
using AccountService.API.Clients.Interfaces;
using AccountService.API.Config;
using AccountService.API.Config.Interfaces;
using AccountService.API.Data;
using AccountService.API.Dto.Request;
using AccountService.API.Repositories;
using AccountService.API.Repositories.Interfaces;
using AccountService.API.Services;
using AccountService.API.Services.Interfaces;
using AccountService.API.Validations;
using Audit.WebApi;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodCommonsLibrary.Core.Middleware;
using PodCommonsLibrary.Core.Utils;
using StackExchange.Redis;

namespace AccountService.API.Middleware;

public static class Registrar
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IMerchantService, MerchantService>()
            .AddScoped<IAccountService, Services.AccountService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddScoped<IDealerService, DealerService>()
            .AddScoped<IBusinessService, BusinessService>()
            .AddScoped<IExpertService, ExpertService>()
            .AddScoped<IOtpService, OtpService>()
            .AddScoped<IAddressService, AddressService>()
            .AddScoped<INotificationService, NotificationService>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<ICustomerRepository, CustomerRepository>()
            .AddScoped<IBusinessRepository, BusinessRepository>()
            .AddScoped<IDealerRepository, DealerRepository>()
            .AddScoped<IExpertRepository, ExpertRepository>()
            .AddScoped<IAddressRepository, AddressRepository>()
            .AddScoped<ICustomerFavouriteDealerMappingRepository, CustomerFavouriteDealerMappingRepository>()
            .AddScoped<IBusinessLocationServiceableCountyRepository, BusinessLocationServiceableCountyRepository>()
            .AddScoped<ICustomerDeliveryInstructionRepository, CustomerDeliveryInstructionRepository>()
            .AddScoped<ICustomerSuggestedDealerRepository, CustomerSuggestedDealerRepository>()
            .AddScoped<TokenConfig>()
            .AddScoped<IValidator<UpdateDealerRequest>, UpdateDealerRequestValidator>()
            .AddScoped<IValidator<UpdateCustomerRequest>, UpdateCustomerRequestValidator>()
            .AddScoped<IValidator<UpdateExpertRequest>, UpdateExpertRequestValidator>()
            .AddScoped<IValidator<ChangeCustomerPasswordRequest>, ChangeCustomerPasswordRequestValidator>()
            .AddScoped<TokenConfig>()
            .AddSingleton<AppConfig>()
            .AddSingleton<IServiceBusConfig, ServiceBusConfig>()
            .AddTransient<DataSeeder>()
            .AddSingleton<HttpClientUtility>();
    }

    public static void RegisterDataServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration["DbContext"];
        services.AddDbContext<AccountServiceDbContext>(
            options => { options.UseNpgsql(connectionString); }
        );
        services.AddMvc(mvc => { mvc.Filters.Add(new AuditIgnoreActionFilter()); });
        services.ConfigureAudit(connectionString);
    }

    public static async Task SeedSampleData(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        DataSeeder dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await dataSeeder.Seed();
    }


    public static void RegisterCacheServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration["RedisContext"];
        // register the cache client
        services.AddStackExchangeRedisCache(options => { options.Configuration = connectionString; });

        // register the redis server
        ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(options);
        EndPoint endPoint = connection.GetEndPoints().First();
        IServer redisServer = connection.GetServer(endPoint);
        services.AddSingleton(redisServer);
    }

    public static void RegisterAuthServices(this IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AccountServiceDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });
    }

    public static void ApplyMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        AccountServiceDbContext db = scope.ServiceProvider.GetRequiredService<AccountServiceDbContext>();
        db.Database.Migrate();
    }

    public static void RegisterClients(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("CatalogServiceBaseUrl"));
            });
        services
            .AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("CustomerServiceBaseUrl"));
            });
        services.AddHttpClient<ISfClient, SfClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("SfBaseUrl"));
        });
        services.AddHttpClient<IVideoCallServiceClient, VideoCallServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("VideoCallServiceBaseUrl"));
        });

        services.AddHttpClient<IJobServiceClient, JobServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("JobServiceBaseUrl"));
        });

        services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("PaymentServiceBaseUrl"));
        });
    }
}
