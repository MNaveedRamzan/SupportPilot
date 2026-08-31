FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything needed for a clean restore + build.
# The .sln/.slnx isn't referenced directly — restoring/building the Api
# project pulls in Domain/Application/Infrastructure via project references.
COPY src/SupportPilot.Domain/ ./src/SupportPilot.Domain/
COPY src/SupportPilot.Application/ ./src/SupportPilot.Application/
COPY src/SupportPilot.Infrastructure/ ./src/SupportPilot.Infrastructure/
COPY src/SupportPilot.Api/ ./src/SupportPilot.Api/

WORKDIR /src/src/SupportPilot.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render provides the PORT env var at runtime; ASP.NET Core needs to be
# told to bind to it explicitly rather than its default port.
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SupportPilot.Api.dll"]