# Contributing to AcademicAI Suite

Thank you for your interest in contributing! This project is licensed under CC BY-NC 4.0 — contributions are welcome for personal and educational use.

## How to Contribute

### Bug Reports
1. Check if the issue already exists in [Issues](https://github.com/MrBlizo/AcademicAI-Suite/issues)
2. Open a new issue with:
   - Clear title and description
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots if applicable
   - Windows version and .NET runtime version

### Feature Requests
1. Open an issue with the `enhancement` label
2. Describe the feature and why it would be useful
3. Include mockups or examples if possible

### Pull Requests
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes
4. Ensure the build passes: `dotnet build AcademicAI.sln`
5. Commit with a descriptive message
6. Open a pull request against `main`

## Development Setup

```bash
git clone https://github.com/MrBlizo/AcademicAI-Suite.git
cd AcademicAI-Suite
dotnet restore AcademicAI.sln
dotnet build AcademicAI.sln
dotnet run --project src/AcademicAI.App/AcademicAI.App.csproj
```

### Prerequisites
- .NET 9 SDK
- Windows 10/11
- Visual Studio 2022 or Rider (optional)

## Code Style

- Follow the `.editorconfig` settings in the repo root
- C# conventions: `PascalCase` for public members, `_camelCase` for private fields
- XAML: 4-space indentation, `DynamicResource` for all theme colors (never hardcoded)
- MVVM pattern: ViewModels use `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]`
- All classes with `[ObservableProperty]` must be `partial`

## Architecture

```
AcademicAI.Core       → Interfaces, models, services (net9.0)
AcademicAI.Agents     → AI provider implementations (net9.0)
AcademicAI.Academic   → Text processors (net9.0)
AcademicAI.Humanizer  → Text humanizer (net9.0)
AcademicAI.Detection  → AI detection (net9.0)
AcademicAI.App        → WPF UI (net9.0-windows)
```

### Key Conventions
- 2 AI providers only: OpenRouter, Fireworks
- Feature-specific agent resolution via `AppSettings.FeatureDefaults`
- AES encryption for secrets (machine-derived key)
- Kill switch reads `control.json` from this repository's `main` branch
