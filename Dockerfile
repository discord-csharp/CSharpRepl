FROM mcr.microsoft.com/dotnet/core/nightly/sdk as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet restore CSDiscord.sln --configfile .nuget/nuget.config
RUN dotnet test CSDiscordService.Tests/CSDiscordService.Tests.csproj
RUN dotnet publish CSDiscordService/CSDiscordService.csproj -o /app
FROM mcr.microsoft.com/dotnet/core/nightly/aspnet
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["bash", "start.sh"]
