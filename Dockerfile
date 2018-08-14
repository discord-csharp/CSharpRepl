FROM microsoft/dotnet:2.1-sdk as dotnet-build
WORKDIR /src
COPY . .
RUN dotnet publish CSDiscord.sln -o /app --configfile .nuget/nuget.config

FROM microsoft/dotnet:2.1.2-aspnetcore-runtime
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["dotnet", "CSDiscordService.dll"]
