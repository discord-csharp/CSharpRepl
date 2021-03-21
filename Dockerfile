FROM mcr.microsoft.com/dotnet/sdk:6.0 as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet --info
RUN dotnet restore CSDiscord.sln --configfile .nuget/nuget.config
RUN dotnet build --configuration Release --no-restore
#RUN dotnet test --configuration Release CSDiscordService.Tests/CSDiscordService.Tests.csproj --no-build --no-restore
RUN dotnet publish --configuration Release CSDiscordService/CSDiscordService.csproj --no-build --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["bash", "start.sh"]
