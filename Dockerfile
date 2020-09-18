FROM mcr.microsoft.com/dotnet/sdk:5.0 as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet --info
RUN dotnet restore CSDiscord.sln --configfile .nuget/nuget.config
RUN dotnet build --no-restore
RUN dotnet test CSDiscordService.Tests/CSDiscordService.Tests.csproj --no-build --no-restore
RUN dotnet publish CSDiscordService/CSDiscordService.csproj --no-build --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN apt-get update && apt-get install -y ca-certificates
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["bash", "start.sh"]
