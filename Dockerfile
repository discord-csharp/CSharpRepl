FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet publish CSDiscord.sln -o /app --configfile .nuget/nuget.config

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["dotnet", "CSDiscordService.dll"]
