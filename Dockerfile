# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TelegramConverterBot/TelegramConverterBot.csproj", "TelegramConverterBot/"]
RUN dotnet restore "TelegramConverterBot/TelegramConverterBot.csproj"

# Copy source code and publish
COPY . .
RUN dotnet publish "TelegramConverterBot/TelegramConverterBot.csproj" -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create directories for temp files and logs
RUN mkdir -p /app/temp /app/logs

# Copy published output from build stage
COPY --from=build /app/out .

# Run the bot
ENTRYPOINT ["dotnet", "TelegramConverterBot.dll"]
