# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY CV_Website.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN useradd -m -u 1000 cvapp && chown -R cvapp:cvapp /app
USER cvapp
COPY --from=build /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 CMD curl -f http://localhost:8080/health || exit 1
EXPOSE 8080
ENTRYPOINT ["dotnet", "CV_Website.dll"]
