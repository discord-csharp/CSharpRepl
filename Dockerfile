FROM microsoft/dotnet:3.0-sdk as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet publish CSDiscord.sln -o /app --configfile .nuget/nuget.config

FROM microsoft/dotnet:3.0-aspnetcore-runtime
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["dotnet", "CSDiscordService.dll"]
