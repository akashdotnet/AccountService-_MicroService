# Account Service

## Pre-requisites

1. Install [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
2. Install [Docker](https://docs.docker.com/desktop/mac/install/).
3. Optionally a database client like [TablePlus](https://tableplus.com/) can be installed to get a better view of the data.
4. Install IDE - Rider or Visual Studio for development.
5. Install [ReportGenerator](https://github.com/danielpalme/ReportGenerator) using the following command
   ```
   dotnet tool install -g dotnet-reportgenerator-globaltool
   ```
6. Install [EF Core tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) as a global tool
    ```
    dotnet tool install --global dotnet-ef
    ```
## Local Development Setup
### Database
1. Setup PostgreSQL locally
2. Connect to PostgreSQL Server in TablePlus by following the instructions from this [article](https://tableplus.com/blog/2018/04/getting-started-with-tableplus.html)
3. Create the API Database using the command
    ```sh 
    createdb AccountService;
    ```
4. Update the DB connection string in `launchSettings.json` file:
    ```
    Server=127.0.0.1,5432;User Id=<user-id>;Password=<YourStrong@Passw0rd>;Database=AccountService
   ```
5. Navigate to the API folder
6. Run the migrations using the command:
   ```sh
   dotnet ef database update
   ```
7. New Migrations can be created with the code first approach using the following command:
   ```sh 
   dotnet ef migrations add <migration_name>
   ```

## How to run service on local

1. Navigate to the API folder
2. Run the project using the following command

   ```sh
   dotnet run --project --launch-profile <profile-name>
   ```
   Choose the profile based on the environment from `launchSettings.json` file.


## Running unit tests locally
1. Run the test cases and generate code coverage report using the following commands:
   ```sh
   dotnet test AccountService.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="../.coverage/Data.opencover.xml" '/p:Include="[*]AccountService.API.Services.*,[*]AccountService.API.Clients.*"' -v quiet --nologo -l:"console;verbosity=normal"
   reportgenerator "-reports:.coverage/Data.opencover.xml" "-targetdir:.coverage-report" -reporttypes:HTML;
   ```
2. Open `.coverage-report/index.html` in your browser to view test coverage report.