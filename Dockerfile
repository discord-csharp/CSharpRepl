FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 as dotnet-build
ARG TARGETPLATFORM
ARG BUILDPLATFORM
WORKDIR /src
COPY . .
RUN dotnet --info
RUN dotnet restore CSharpRepl.sln
RUN dotnet build --configuration Release --no-restore
# i guess we sitll need to skip tests in CI (github actions or azure devops), anyone have any ideas?
#RUN dotnet test --configuration Release CSharpRepl.Tests/CSharpRepl.Tests.csproj --no-build --no-restore
RUN dotnet publish --configuration Release CSharpRepl/CSharpRepl.csproj --no-build --no-restore -o /app

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:7.0
ARG TARGETPLATFORM
ARG BUILDPLATFORM
WORKDIR /app
COPY --from=dotnet-build /app .
ENTRYPOINT ["bash", "start.sh"]
