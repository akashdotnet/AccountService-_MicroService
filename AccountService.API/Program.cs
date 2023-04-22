using System;
using System.Text.Json.Serialization;
using AccountService.API.Middleware;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PodCommonsLibrary.Core.Exceptions;
using PodCommonsLibrary.Core.Middleware;
using Serilog;

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Logging Configuration
    builder.Host.UseSerilog();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    Log.Information("Starting up");

    builder.Services.AddControllers()
        .AddFluentValidation()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .ConfigureApiBehaviorOptions(options =>
        {
            // by default invalid state returns BadRequestObjectResult, overriding this behaviour to throw CustomValidationException to ensure a common pattern in error response 
            options.InvalidModelStateResponseFactory = context =>
                throw new CustomValidationException(context.ModelState);
        });

    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.RegisterServices();
    builder.Services.RegisterDataServices(builder.Configuration);
    builder.Services.RegisterClients(builder.Configuration);
    builder.Services.RegisterCacheServices(builder.Configuration);
    builder.Services.RegisterAuthServices();

    // used in the audits to obtain username from the http context
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSwaggerGeneration();
    builder.Services.AddHealthChecks();

    WebApplication app = builder.Build();

    app.UseSerilogRequestLogging();
    // Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

    app.Use(async (context, next) =>
    {
        // to ensure that the request body is read both by audit logger and controller ref: https://github.com/thepirat000/Audit.NET/tree/master/src/Audit.WebApi#note
        context.Request.EnableBuffering();
        await next();
    });

    app.UseAuditMiddleware();
    app.UseCustomExceptionHandler();
    app.UseHttpsRedirection();
    app.MapControllers();
    app.ApplyMigrations();
    await app.SeedSampleData();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
