# ClipHist

A tiny, privacy-friendly clipboard history tool for Windows. It keeps your **last 5 copied text snippets** in memory and lets you paste any of them back with a single keystroke — no account, no network, no disk writes.

## Features

- 🕶️ **Background clipboard monitor** — records Unicode plain text only (ignores empty content; keeps line breaks and spaces intact).
- 🔢 **Last 5 items** — newest first; re-copying an existing item moves it to the top without taking a new slot.
- ⚡ **Global hotkey `Ctrl + Shift + V`** — opens a borderless popup near your mouse cursor.
- 🖱️ **Pick & paste** — press `1`–`5` or click an item to paste it into the current input.
- 🚫 **Closes on** `Esc`, focus loss, or clicking outside the panel.
- 🧠 **Memory-only** — history is cleared on exit; nothing is written to disk; no auto-start.

## Download

Grab the latest build from the [Releases](https://github.com/xlc0007/ClipHist/releases) page:

- `ClipHist_v1.0.0_win-x64.zip` — self-contained, no .NET runtime required.

## How to use

1. **Run** `ClipHist.exe` (double-click). It lives in the system tray — no window.
2. **Copy** text normally (`Ctrl+C`); the last 5 items are recorded automatically.
3. Press **`Ctrl + Shift + V`** → a popup appears near your cursor (`1` = newest, `5` = oldest).
4. Press a number key or click an item → it's pasted instantly.
5. Tray menu: **Pause/Resume recording** · **Exit**.

## ⚠️ Windows SmartScreen notice

This app is **not code-signed**, so Windows may show a "Windows protected your PC" warning on first run. That's expected for free, unsigned tools. To run it:

1. Click **"More info"**.
2. Click **"Run anyway"**.

(If you prefer a signed build, see the note in [BUILD.md](BUILD.md).)

## Limitations

- Windows 10/11 only; plain text only (no images, files, or rich text).
- Auto-paste relies on the target app accepting a standard `Ctrl+V`. If an app blocks simulated input, the text is still copied to the clipboard — just paste manually.

## Build from source

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```

Requires the .NET 8 SDK.

## License

[MIT](LICENSE)
