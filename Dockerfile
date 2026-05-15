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
RUN useradd -m cvapp \
    && mkdir -p /data \
    && chown -R cvapp:cvapp /app /data
COPY --chown=cvapp:cvapp --from=build /app/publish .
USER cvapp
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "mkdir -p /data/dp-keys 2>/dev/null || true; dotnet CV_Website.dll"]
