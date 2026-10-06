# TASY Motion Diet

**Turn oversized generated video into compact web animation.**

3D and generative applications often create video files that are far larger than necessary for a website, forum, preview or quick demonstration. Motion Diet provides a tiny Windows interface for converting those files through FFmpeg without rebuilding command lines every time.

## What it does

- input: MP4 / MOV / MKV / WebM / AVI / M4V
- output: Animated WebP or GIF
- maximum long side: 480 px, no upscaling
- Lanczos resize
- removes audio and metadata
- infinite animation loop
- leaves the source video unchanged

## Presets

| Preset | Size / FPS | Animated WebP | GIF |
|---|---|---|---|
| ULTRA BEAUTY | 480 px / 12 fps | quality 100 | 256 colors |
| BEAUTY | 480 px / 12 fps | quality 95 | 256 colors |
| LIGHT | 480 px / 8 fps | quality 90 | 256 colors |
| UI | 480 px / 10 fps | quality 70 | 128 colors |

![TASY Motion Diet](../../assets/motion-diet/ui.png)

### Reading the presets

The **480 px** value is a maximum for the longest side, not a forced resolution; smaller sources are not enlarged. FPS controls temporal sampling and therefore strongly affects output weight. WebP quality controls lossy Animated WebP encoding. GIF has no equivalent continuous quality slider here, so the presets use palette size instead.

Published examples will include the complete measurement line: **source resolution · duration · source bytes → preset · output format · output resolution · output bytes**. A reduction factor is shown only when both matching files are available for measurement.

## FFmpeg lookup

The program searches in this order:

1. `tools\\ffmpeg.exe`
2. `ffmpeg.exe` beside the application
3. system `PATH`

## Build

Run [`BUILD-EXE.bat`](BUILD-EXE.bat). It uses the classic .NET Framework C# compiler from the standard Windows locations and creates `TASY-Motion-Diet.exe` in the same folder. No IDE is required.

## Files

- [`TASY-Motion-Diet.cs`](TASY-Motion-Diet.cs) — C# / WinForms source
- [`BUILD-EXE.bat`](BUILD-EXE.bat) — Windows build script

FFmpeg is not bundled. See the repository [THIRD_PARTY.md](../../THIRD_PARTY.md).
