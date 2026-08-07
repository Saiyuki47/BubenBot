# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the solution from .NET Core 3.1 to .NET 10.0 (LTS) and refresh the small dependency set that needs version alignment.
**Scope**: 1 SDK-style project, minimal dependency surface, small codebase.

## Tasks

### 01-upgrade-bubenbot-project
Upgrade `BubenBot.csproj` to `net10.0`, update the recommended NuGet package versions for .NET 10 compatibility, and fix any compile-time issues introduced by the framework jump.

This project is small and self-contained, but it has at least one source-incompatible API usage that must be recompiled and validated after the framework change.

**Done when**: the project targets `net10.0`, the recommended package update is applied, the project builds successfully, and any framework-related compile errors are resolved.
