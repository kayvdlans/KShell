# KShell

A minimal shell written in C# (.NET 10).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Features

- **Built-in commands:** `cd`, `echo`, `exit`, `history`, `pwd`, `type`
- **External commands:** Runs executables from `PATH` (e.g. `ls`, `cat`)
- **Pipelines:** Chain commands with `|` (e.g. `echo hello | cat`)
- **Redirection:** `>`, `>>`, `2>`, `2>>` for stdout/stderr
- **History:** Up/Down arrows to browse previous commands
- **Tab completion:** Completes built-in command names

## Run

```bash
dotnet run --project KShell
```

## Build

```bash
dotnet build KShell
```
