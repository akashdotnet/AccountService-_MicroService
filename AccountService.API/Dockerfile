FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | sh
ARG FEED_ACCESSTOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS \
    "{\"endpointCredentials\": [{\"endpoint\":\"https://sportscrm.pkgs.visualstudio.com/75d8e0cd-c6f9-4c3f-a2c8-f4438b0aece4/_packaging/pod-commons/nuget/v3/index.json\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"
COPY ["AccountService.API.csproj", "AccountService.API/"]
COPY ["nuget.config", ""]
RUN dotnet restore "AccountService.API/AccountService.API.csproj"
COPY . ./AccountService.API
WORKDIR "/src/AccountService.API"

FROM build AS publish
RUN dotnet publish "AccountService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.API.dll"]