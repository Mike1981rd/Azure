FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip

# Copy csproj and restore
COPY ["WebsiteBuilderAPI.csproj", "./"]
RUN dotnet restore "WebsiteBuilderAPI.csproj" --disable-parallel --nologo

# Copy everything else and build
COPY . .
RUN dotnet publish "WebsiteBuilderAPI.csproj" -c Release -o /app/publish --no-restore --nologo

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "WebsiteBuilderAPI.dll"]
