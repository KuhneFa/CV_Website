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
RUN useradd -m -u 1000 cvapp \
    && mkdir -p /data \
    && chown -R cvapp:cvapp /app /data
USER cvapp
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CV_Website.dll"]
