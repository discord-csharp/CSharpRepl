FROM mcr.microsoft.com/dotnet/sdk:5.0 as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet --info
RUN dotnet restore CSDiscord.sln --configfile .nuget/nuget.config
RUN dotnet test CSDiscordService.Tests/CSDiscordService.Tests.csproj
RUN dotnet publish CSDiscordService/CSDiscordService.csproj -o /app
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["bash", "start.sh"]
