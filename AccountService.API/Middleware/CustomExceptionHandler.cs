using System;
using AccountService.API.Constants;
using AccountService.API.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PodCommonsLibrary.Core.Dto;
using PodCommonsLibrary.Core.Middleware;

namespace AccountService.API.Middleware;

public static class CustomExceptionHandler
{
    private static ExceptionResponse? GetExceptionResponse(Exception exception)
    {
        ErrorResponse errorResponse = new()
        {
            Message = exception.Message
        };
        switch (exception)
        {
            case ServiceUnavailableException serviceUnavailableException:
                errorResponse.ErrorResponseType = StaticValues.ServiceUnavailable;
                errorResponse.Message = serviceUnavailableException.HealthReportStatus;
                return new ExceptionResponse
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    ErrorResponse = errorResponse
                };
        }

        return null;
    }

    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>(GetExceptionResponse);
    }
}
