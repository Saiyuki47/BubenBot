# 01-upgrade-bubenbot-project: Upgrade BubenBot Project

Upgrade `BubenBot.csproj` to `net10.0`, update the recommended NuGet package versions for .NET 10 compatibility, and fix any compile-time issues introduced by the framework jump.

This project is small and self-contained, but it has at least one source-incompatible API usage that must be recompiled and validated after the framework change.

## Research notes
- The project has a single SDK-style project file: `BubenBot.csproj`.
- `get_project_dependencies` shows package versions are defined directly in the project file, not via CPM.
- Assessment identified one package with a recommended update: `Microsoft.Extensions.DependencyInjection` → `10.0.8`.
- Assessment also flagged `M:System.IO.FileInfo.ToString` as a source-incompatible API usage. In `Commands.cs`, the meme command was using `FileInfo.ToString()` when passing a file to `SendFileAsync`; this was updated to `FullName` so the sender gets an actual path.

## Execution plan
1. Update `TargetFramework` to `net10.0`.
2. Update `Microsoft.Extensions.DependencyInjection` to the recommended version.
3. Fix the source-incompatible `FileInfo.ToString()` usage.
4. Build and validate the project.

**Done when**: the project targets `net10.0`, the recommended package update is applied, the project builds successfully, and any framework-related compile errors are resolved.
