# Telegram File Converter Bot

A production-ready Telegram Bot built with C# .NET 8 that converts files between multiple formats. Users simply send a file and the bot provides inline buttons for available conversion options.

## Supported Conversions

| Source | Target |
|--------|--------|
| 📄 DOCX | PDF |
| 📕 PDF | DOCX |
| 📊 XLSX | PDF |
| 📽 PPTX | PDF |
| 🖼 Image (JPG/PNG) | PDF |

## Features

- ✅ Automatic file type detection (extension + MIME type)
- ✅ Inline keyboard with conversion options
- ✅ File size validation (configurable max size)
- ✅ Temporary file management with automatic cleanup
- ✅ Comprehensive error handling
- ✅ Structured logging with Serilog
- ✅ Dependency Injection throughout
- ✅ Async/await architecture
- ✅ Docker support for easy deployment

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Telegram Bot Token (get one from [@BotFather](https://t.me/BotFather))

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TelegramConverterBot
```

### 2. Configure the Bot

Edit `appsettings.json` and replace the bot token:

```json
{
  "BotToken": "YOUR_TELEGRAM_BOT_TOKEN_HERE",
  "TempFilesPath": "temp/",
  "MaxFileSizeMB": 20
}
```

**Alternative:** Set the token via environment variable:

```bash
# Windows (PowerShell)
$env:BotToken="your-bot-token-here"

# Linux/macOS
export BotToken="your-bot-token-here"
```

### 3. Restore NuGet Packages

```bash
dotnet restore
```

### 4. Run the Bot

```bash
dotnet run
```

You should see output similar to:

```
🚀 Starting Telegram Converter Bot...
🤖 Bot started! Username: your_bot_name, Name: Your Bot
Bot is now polling for updates...
```

### 5. Test in Telegram

1. Open Telegram and find your bot
2. Send `/start` to see the welcome message
3. Send any supported file (DOCX, PDF, PPTX, XLSX, JPG, PNG)
4. Click the conversion button that appears
5. Receive your converted file!

## Building for Production

### Publish a Self-Contained Executable

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained -o ./publish

# Linux
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish

# macOS
dotnet publish -c Release -r osx-x64 --self-contained -o ./publish
```

### Run the Published Build

```bash
# Windows
./publish/TelegramConverterBot.exe

# Linux/macOS
./publish/TelegramConverterBot
```

## Docker Deployment

### 1. Build the Image

```bash
docker build -t telegram-converter-bot .
```

### 2. Run the Container

```bash
docker run -d \
  --name converter-bot \
  -e BotToken="your-bot-token-here" \
  -v ./temp:/app/temp \
  -v ./logs:/app/logs \
  telegram-converter-bot
```

### 3. Docker Compose (Optional)

Create a `docker-compose.yml`:

```yaml
version: '3.8'

services:
  bot:
    build: .
    container_name: telegram-converter-bot
    environment:
      - BotToken=your-bot-token-here
    volumes:
      - ./temp:/app/temp
      - ./logs:/app/logs
    restart: unless-stopped
```

Then run:

```bash
docker compose up -d
```

## Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| `BotToken` | Your Telegram bot token from BotFather | *(required)* |
| `TempFilesPath` | Directory for temporary file storage | `temp/` |
| `MaxFileSizeMB` | Maximum allowed file size in MB | `20` |

## Project Structure

```
TelegramConverterBot/
├── Program.cs                          # Entry point, DI setup
├── appsettings.json                    # Configuration
├── Bot/
│   ├── BotService.cs                   # IHostedService, polling loop
│   ├── UpdateHandler.cs                # Handles messages & callbacks
│   └── CallbackHandler.cs              # Handles inline button callbacks
├── Services/
│   ├── FileDetectorService.cs          # File type detection
│   ├── ConversionService.cs            # Conversion routing
│   └── TempFileService.cs              # Temp file management
├── Converters/
│   ├── IConverter.cs                   # Converter interface
│   ├── DocxToPdfConverter.cs           # DOCX → PDF
│   ├── PdfToDocxConverter.cs           # PDF → DOCX
│   ├── PptxToPdfConverter.cs           # PPTX → PDF
│   ├── XlsxToPdfConverter.cs           # XLSX → PDF
│   └── ImageToPdfConverter.cs          # Image → PDF
├── Models/
│   ├── ConversionJob.cs                # Job metadata
│   ├── FileType.cs                     # Source file type enum
│   └── ConversionTarget.cs             # Target format enum
└── Helpers/
    ├── KeyboardBuilder.cs              # Inline keyboard builder
    └── FileHelper.cs                   # File download utilities
```

## Libraries Used

| Library | Purpose |
|---------|---------|
| **Telegram.Bot** | Telegram Bot API client |
| **DocumentFormat.OpenXml** | Reading/writing DOCX, PPTX, XLSX |
| **iText7** | PDF creation and reading |
| **EPPlus** | Excel file reading |
| **Serilog** | Structured logging |
| **Microsoft.Extensions.*** | DI, Configuration, Hosting |

## Logging

Logs are written to:
- **Console**: Real-time output
- **File**: `logs/bot-YYYYMMDD.txt` (rolled daily)

## Error Handling

The bot handles these scenarios gracefully:

- ❌ File too large → Shows max size message
- ❌ Unsupported format → Shows unsupported message
- ❌ No file uploaded (text message) → Prompts to send a file
- ❌ Conversion failure → Shows error message
- ❌ Stale callback (no active job) → Session expired message
- ✅ Always answers callback queries to prevent Telegram timeouts

## Troubleshooting

### Bot doesn't respond
- Verify the `BotToken` in `appsettings.json` is correct
- Check the console for startup errors
- Ensure the bot is not already running elsewhere

### Conversion fails
- Check `logs/` directory for detailed error logs
- Verify the uploaded file is not corrupted
- Ensure file is within the max size limit

### Docker build fails
- Ensure Docker is installed and running
- Check that you have sufficient disk space
- Verify the `appsettings.json` file exists

## License

This project is provided as-is for educational and personal use.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.
