# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Catapult is a Windows application launcher (similar to Alfred/Spotlight) built with C# and WPF. It provides fast file/app search with fuzzy matching, global hotkeys (Alt+Space), browser bookmark indexing, and system actions. The app uses frecency-based ranking (frequency + recency) and supports extensible actions.

## Build Commands

```bash
# Build entire solution
dotnet build src/Catapult.sln

# Build specific configuration  
dotnet build src/Catapult.sln -c Release

# Build for specific platform
dotnet build src/Catapult.sln -c Release -p:Platform=x64

# Run tests
dotnet test src/Catapult.Tests/Catapult.Tests.csproj

# Run tests with coverage
dotnet test src/Catapult.Tests/Catapult.Tests.csproj --collect:"XPlat Code Coverage"

# Run the application (from src directory)
dotnet run --project Catapult.App/Catapult.App.csproj

# Package for Windows Store (requires Visual Studio or msbuild)
msbuild src/Catapult.Package/Catapult.Package.wapproj -p:Configuration=Release -p:Platform=x64
```

## Architecture

### Project Structure
- **Catapult.Core**: Business logic, search engine, actions framework, indexing
- **Catapult.App**: WPF UI, ViewModels, Windows-specific integrations  
- **Catapult.Tests**: NUnit tests with Should assertion library
- **Catapult.Package**: MSIX packaging for Windows Store distribution

### Key Architecture Patterns
- **Actions System**: Extensible via `IAction`, `IIndexable`, `IConvert` interfaces
- **MVVM**: ViewModels for MainWindow, ListWindow, DetailsWindow
- **Search Engine**: Selecta-based fuzzy matching with frecency scoring
- **Icon Resolution**: Multi-strategy pattern (file icons, favicons, URLs)
- **Indexing**: Background file system and bookmark scanning

### Core Components
- **ActionRegistry**: Central registration and discovery of actions in `src/Catapult.Core/Actions/ActionRegistry.cs:*`
- **Searcher**: Main search logic in `src/Catapult.Core/Selecta/Searcher.cs:*`
- **MainViewModel**: Primary UI controller in `src/Catapult.App/MainViewModel.cs:*`
- **IndexStore**: Search index management in `src/Catapult.Core/Indexes/IndexStore.cs:*`

## Development Guidelines

### Technologies
- **.NET 9.0** (recently upgraded from .NET 8.0)
- **WPF** for Windows UI
- **NUnit 3.14** for testing with Should assertions
- **Serilog** for structured logging
- **Newtonsoft.Json** for configuration

### Code Conventions
- Interfaces prefixed with `I` (e.g., `IAction`, `IIndexable`)
- Actions follow `{ActionName}Action.cs` naming
- ViewModels follow `{View}ViewModel.cs` pattern
- Use nullable reference types and implicit usings
- Follow existing MVVM patterns for UI code

### Adding New Actions
1. Implement `IAction` interface in `src/Catapult.Core/Actions/`
2. Register in `ActionRegistry.cs` constructor
3. Add any required indexing logic if implementing `IIndexable`
4. Add tests in `src/Catapult.Tests/`

### Testing Specific Classes
```bash
# Run tests for specific class
dotnet test src/Catapult.Tests/ --filter "ClassName=ActionRegistryTests"

# Run single test method
dotnet test src/Catapult.Tests/ --filter "TestMethodName=ShouldFindAction"
```

### Platform Requirements
- **Windows 10/11** (due to WPF and Windows APIs)
- **Visual Studio 2022** or **JetBrains Rider**
- **.NET 9.0 SDK**

## Key Implementation Details

### Single Instance Management
Application uses mutex to prevent multiple instances in `src/Catapult.App/Program.cs:*`

### Global Hotkey Registration  
Hotkeys registered via NHotkey.Wpf in `src/Catapult.App/MainWindow.xaml.cs:*`

### Configuration System
JSON-based user settings in `src/Catapult.Core/Config/JsonUserConfiguration.cs:*`

### Logging
Rolling file logs with in-app viewer at `src/Catapult.App/LogWindow.xaml:*`

### Frecency Algorithm
Usage frequency + recency scoring in `src/Catapult.Core/Frecency/FrecencyStorage.cs:*`

### Auto-Update System
MSIX-based auto-update with GitHub releases:
- **App Installer**: Configured in `src/Catapult.Package/Catapult.Package.wapproj:60` for web deployment
- **Update Service**: GitHub API integration in `src/Catapult.Core/Services/UpdateService.cs:*`
- **Update Action**: User-triggered update check in `src/Catapult.Core/Actions/CheckForUpdatesAction.cs:*`
- **Build Pipeline**: Automated MSIX builds in `.github/workflows/build-and-release.yml`

#### Triggering Updates
- Search "Check for Updates" in Catapult
- Updates install via Windows App Installer protocol (`ms-appinstaller:`)
- Automatic update checks on application launch (configured in .wapproj)