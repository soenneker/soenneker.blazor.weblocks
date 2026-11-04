[![](https://img.shields.io/nuget/v/soenneker.blazor.weblocks.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.weblocks/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.weblocks/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.weblocks/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.weblocks.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.weblocks/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.weblocks)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.weblocks/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.weblocks/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.WebLocks
### Blazor interop for the browser Web Locks API.

## Installation

```bash
dotnet add package Soenneker.Blazor.WebLocks
```

## Setup

Register services in `Program.cs`:

```csharp
builder.Services.AddWebLocksAsScoped();
```

Inject the higher-level utility where you need it:

```csharp
@inject IWebLocks WebLocks
```

## Usage

Initialize the package once before first use:

```csharp
await WebLocks.Initialize();
```
