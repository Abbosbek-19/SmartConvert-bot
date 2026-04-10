# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TelegramConverterBot.csproj", "./"]
RUN dotnet restore "TelegramConverterBot.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "TelegramConverterBot.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create directories for persistent data
RUN mkdir -p /app/temp /app/logs

# Copy published app from build stage
COPY --from=build /app/publish .

# Set environment variables
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose no ports (bot uses polling, not webhooks)
# EXPOSE 80

# Health check (checks if process is still running)
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD pgrep -f TelegramConverterBot || exit 1

# Run the application
ENTRYPOINT ["dotnet", "TelegramConverterBot.dll"]
