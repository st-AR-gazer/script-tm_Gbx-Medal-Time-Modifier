# GbxMedalTimeModifier

Standalone CLI for editing medal times in Trackmania `.Gbx` map files using [GBX.NET](https://github.com/BigBang1112/GBX.NET).

## Features

- Set custom medal times for Author (AT), Gold, Silver, and Bronze.
- Use `_` to leave a medal unchanged.
- Use `auto` for Gold, Silver, and Bronze to generate Trackmania-style defaults from AT.
- Native batch mode for processing folders of maps (Python script no longer needed).

Auto multipliers:

- Gold = AT x 1.06
- Silver = AT x 1.20
- Bronze = AT x 1.50

## Build and run

```powershell
# Build
dotnet build

# Run
dotnet run -- <args>
```

## Usage

Single map:

```powershell
GbxMedalTimeModifier.exe <inputMapPath> <outputMapPath> <AT> <Gold> <Silver> <Bronze>
```

Batch mode:

```powershell
GbxMedalTimeModifier.exe --batch <inputDir> <outputDir> <AT> <Gold> <Silver> <Bronze> [--recursive] [--pattern <glob>]
```

Notes:

- `<AT>` cannot be `auto`.
- If `<inputMapPath>` is a directory, batch mode is enabled automatically.
- Default batch pattern is `*.Gbx`.

Examples:

```powershell
# Single map
GbxMedalTimeModifier.exe "C:\Maps\MyMap.Gbx" "C:\Maps\Out\MyMap.Gbx" 60000 auto auto _

# Batch folder
GbxMedalTimeModifier.exe --batch "C:\Maps\Input" "C:\Maps\Output" 60000 auto auto _ --recursive --pattern "*.Gbx"
```

## Standalone EXE publish

From repo root:

```powershell
.\publish-standalone.ps1
```

This publishes a self-contained single-file executable to:

- `dist\win-x64\GbxMedalTimeModifier.exe`

It also copies the published exe to the repo root:

- `GbxMedalTimeModifier.exe`

## License

This project uses `GBX.NET` (MIT) and `GBX.NET.LZO` (GPL-3.0-or-later).

Because `GBX.NET.LZO` is included for compressed `.Gbx` support, this project and distributed binaries must comply with GNU GPL v3 (or later).
