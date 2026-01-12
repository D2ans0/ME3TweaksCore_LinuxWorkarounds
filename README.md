# ME3TweaksCore

[![GitHub](https://img.shields.io/github/license/ME3Tweaks/ME3TweaksCore)](https://github.com/ME3Tweaks/ME3TweaksCore)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)

ME3TweaksCore is a comprehensive .NET library that provides core functionality for ME3Tweaks software tools. It offers a robust set of features for working with Mass Effect games, including mod management, game file manipulation, diagnostics, and texture overrides.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Project Structure](#project-structure)
- [Key Components](#key-components)
- [Technologies](#technologies)
- [Usage](#usage)
- [Building](#building)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Library (ME3TweaksCore)

- **Game Target Management**: Comprehensive support for Mass Effect trilogy (ME1, ME2, ME3) and Legendary Edition (LE1, LE2, LE3)
- **Mod Management**: 
  - DLC mod installation and management
  - ASI mod support and management
  - Native mod handling
  - Third-party mod identification
- **M3Merge System**: Advanced merging capabilities for:
  - Bio2DA tables
  - Global shaders
  - Plot managers
  - Squadmate outfits
  - LE1 configuration
  - ME2 email system
  - Startup files
- **Diagnostics & Logging**:
  - Comprehensive diagnostic tools
  - Game file verification
  - TOC (Table of Contents) validation
  - Event log diagnostics
  - MEM (Mass Effect Modder) integration
  - Log collection and upload
- **Backup & Restore Services**:
  - Game backup creation
  - Game restoration from backups
  - File source management
- **Texture Management**:
  - Texture override compilation
  - Binary texture package handling
  - LOD (Level of Detail) settings
  - Texture mod installation tracking
- **Game Filesystem Operations**:
  - File validation and verification
  - SFAR archive manipulation
  - Cooked file management
  - DLC path handling
  - Bink video support
- **Configuration Management**:
  - Config file merging
  - Settings persistence
  - Localization support (multiple languages: English, Italian, German, Russian)
- **Online Services Integration**:
  - Update checking
  - Content delivery
  - Telemetry support
- **Starter Kit**:
  - DLC mod generation
  - Template files
  - Development tools

### WPF Extensions (ME3TweaksCoreWPF)

- **UI Components**: WPF-specific controls and utilities
- **Log Viewer**: Web-based log viewing interface
- **Visual Helpers**: UI converters and services

## Requirements

- **.NET 10.0 or later** (Windows platform)
- **Windows operating system**
- **Visual Studio 2022 or later** (for development)
- **x64 architecture**

### Dependencies

The library includes the following submodules and packages:

**Submodules:**
- [LegendaryExplorer](https://github.com/ME3Tweaks/LegendaryExplorer) - Core Mass Effect package handling
- [ComputerInfo](https://github.com/ME3Tweaks/ComputerInfo) - System information utilities
- [RoboSharp](https://github.com/Mgamerz/RoboSharp) - Robust file copying
- [AuthenticodeExaminer](https://github.com/ME3Tweaks/AuthenticodeExaminer) - Code signing verification

**NuGet Packages:**
- CliWrap (3.10.0) - Command-line interface wrapping
- Flurl (4.0.0) / Flurl.Http (4.0.2) - HTTP client library
- Octokit (14.0.0) - GitHub API client
- PropertyChanged.Fody (4.1.0) - Property change notification
- System.Diagnostics.EventLog (10.0.0) - Windows Event Log access
- System.Management (10.0.0) - WMI support

## Installation

### As a Library Dependency

To use ME3TweaksCore in your project, you can reference it as a project dependency or build it as a library:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\ME3TweaksCore\ME3TweaksCore.csproj" />
</ItemGroup>
```

### Cloning the Repository

```bash
git clone --recursive https://github.com/ME3Tweaks/ME3TweaksCore.git
cd ME3TweaksCore
```

**Note:** Use `--recursive` to ensure all submodules are cloned.

## Project Structure

```
ME3TweaksCore/
├── ME3TweaksCore/              # Core library project
│   ├── Assets/                 # Embedded assets and resources
│   ├── Build/                  # Build scripts
│   ├── Config/                 # Configuration management
│   ├── Diagnostics/            # Diagnostic tools and modules
│   ├── GameFilesystem/         # Game filesystem operations
│   ├── Helpers/                # Utility helper classes
│   ├── Localization/           # Multi-language support
│   ├── ME3Tweaks/              # ME3Tweaks-specific features
│   │   ├── M3Merge/           # Merge system implementations
│   │   ├── ModManager/        # Mod management interfaces
│   │   ├── Online/            # Online services
│   │   └── StarterKit/        # Mod development tools
│   ├── Misc/                   # Miscellaneous utilities
│   ├── NativeMods/             # ASI and native mod support
│   ├── Objects/                # Core data objects
│   ├── Save/                   # Save game utilities
│   ├── Services/               # Service layer
│   │   ├── Backup/            # Backup services
│   │   ├── FileSource/        # File source management
│   │   ├── Restore/           # Restore services
│   │   └── Shared/            # Shared services
│   ├── Targets/                # Game target management
│   ├── TextureOverride/        # Texture override system
│   └── submodules/             # Git submodules
├── ME3TweaksCoreWPF/           # WPF UI extensions
├── ME3TweaksCore.Test/         # Unit tests
└── ME3TweaksCore.sln           # Visual Studio solution
```

## Key Components

### ME3TweaksCoreLib

The main initialization class for the library. You must call `Initialize()` before using any library features:

```csharp
using ME3TweaksCore;

var initPackage = new ME3TweaksCoreLibInitPackage
{
    // Configure initialization parameters
    GetLogger = () => yourLogger,
    LoadAuxiliaryServices = true,
    // ... other options
};

ME3TweaksCoreLib.Initialize(initPackage);
```

### GameTarget

Represents a Mass Effect game installation. Provides access to game files, DLC, and configuration:

```csharp
var gameTarget = new GameTarget(MEGame.LE3, gamePath, registryActive: false);
```

### M3Merge

The merging system allows mods to modify game assets in a compatible way:
- **Bio2DAMerge**: Merge 2DA database tables
- **PlotManagerMerge**: Merge plot manager data
- **SQMOutfitMerge**: Merge squadmate outfit configurations
- **GlobalShaderMerge**: Merge shader configurations

### ASI Manager

Manages ASI (plugin) mods for Mass Effect games:

```csharp
var asiManager = new ASIManager();
var installedMods = asiManager.GetInstalledASIs(gameTarget);
```

### Diagnostics

Comprehensive diagnostic capabilities for troubleshooting:
- Game installation validation
- Mod conflict detection
- File integrity checking
- Log collection and analysis

## Technologies

- **Language**: C# 10+
- **Framework**: .NET 10.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Logging**: Serilog
- **Version Control**: Git with submodules
- **Package Management**: NuGet
- **Build System**: MSBuild

## Usage

### Basic Initialization

```csharp
// Create initialization package
var initPackage = new ME3TweaksCoreLibInitPackage
{
    GetLogger = () => CreateLogger(),
    LoadAuxiliaryServices = true,
    AuxiliaryCombinedOnlineServicesEndpoint = "https://api.me3tweaks.com"
};

// Initialize the library
ME3TweaksCoreLib.Initialize(initPackage);

// Now you can use the library features
var version = ME3TweaksCoreLib.CoreLibVersionHR;
Console.WriteLine($"Initialized {version}");
```

### Working with Game Targets

```csharp
// Create a game target
var target = new GameTarget(MEGame.LE3, @"C:\Games\Mass Effect Legendary Edition\Game\ME3", false);

// Get game information
var executablePath = target.GetExecutablePath();
var dlcPath = target.GetDLCPath();
var cookedPath = target.GetCookedPath();

// Validate game installation
bool isValid = target.ValidateTarget();
```

## Building

### Prerequisites

1. Install Visual Studio 2022 or later with .NET 10.0 SDK
2. Clone the repository with submodules: `git clone --recursive https://github.com/ME3Tweaks/ME3TweaksCore.git`
3. Ensure all submodules are initialized: `git submodule update --init --recursive`

### Build Steps

#### Using Visual Studio
1. Open `ME3TweaksCore.sln` in Visual Studio
2. Restore NuGet packages (should happen automatically)
3. Build the solution (Ctrl+Shift+B)

#### Using Command Line
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build ME3TweaksCore.sln --configuration Release
```

### Build Configurations

The solution supports multiple build configurations:
- **Debug**: Development builds with full debugging symbols
- **Release**: Optimized production builds
- **LinuxDebug/LinuxRelease**: Linux platform builds
- **MacDebug/MacRelease**: macOS platform builds

### Localization Build

The project includes a PowerShell script that runs before compilation to prepare localization files:

```powershell
# Automatically run during build
.\ME3TweaksCore\Build\preparelocalizations.ps1
```

## Contributing

Contributions are welcome! This library is part of the ME3Tweaks ecosystem and follows these guidelines:

1. **Code Style**: Follow existing code conventions and patterns
2. **Testing**: Include unit tests for new features when applicable
3. **Documentation**: Update relevant documentation for significant changes
4. **Submodules**: Be careful when updating submodules to ensure compatibility
5. **Localization**: Consider localization impact for user-facing strings

### Development Workflow

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

Copyright 2021-2026 ME3Tweaks

This project is part of the ME3Tweaks software suite. Please refer to the repository for specific license information.

## Related Projects

- [Mass Effect 3 Mod Manager](https://github.com/ME3Tweaks/ME3TweaksModManager) - The flagship mod manager for Mass Effect games
- [LegendaryExplorer](https://github.com/ME3Tweaks/LegendaryExplorer) - Toolset for exploring and editing Mass Effect packages
- [Mass Effect Modder](https://github.com/ME3Tweaks/MassEffectModder) - Texture modding tools

## Support

For support and questions:
- Visit the [ME3Tweaks Discord](https://discord.gg/me3tweaks)
- Check the [ME3Tweaks website](https://me3tweaks.com)
- Open an issue on GitHub

## Acknowledgments

This library builds upon and integrates with several open-source projects and the Mass Effect modding community's collective knowledge. Special thanks to all contributors and the Mass Effect modding community.
