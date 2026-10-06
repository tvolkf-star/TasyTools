# TASY Image Diet

**Batch image weight reduction for Windows.**

Image Diet is for the ordinary situation where an image is much heavier than its real use requires: website images, forum uploads, previews, textures, portfolios and image exchange over a slow connection.

## What it does

- accepts PNG / JPG / JPEG files
- accepts individual files or a folder for batch processing
- converts output to JPEG
- limits the longest side to 1400 px without upscaling
- removes metadata
- can delete originals after successful processing if explicitly enabled
- reports total size before → after

Real-world reduction depends on the source. Typical working examples range from roughly **2× to 20× smaller** with little visible difference at normal screen viewing size.

## Example

Three measured pairs are included:

| Example | Original | Result | Reduction |
|---|---:|---:|---:|
| `2026-10-06_21-50-05` | 2015×1084 PNG · 2,990,005 bytes | 1400×754 JPEG · 223,483 bytes | **13.4×** |
| `ComfyUI_00505_` | 1024×1024 PNG · 1,716,231 bytes | 1024×1024 JPEG · 214,287 bytes | **8.0×** |
| `ComfyUI_00419_` | 1024×1024 PNG · 1,343,370 bytes | 1024×1024 JPEG · 137,646 bytes | **9.8×** |

The two 1024×1024 examples are not resized, which makes them useful demonstrations of the reduction produced by JPEG output and metadata handling alone. The 2015×1084 source is resized to 1400×754 because its longest side exceeds the configured 1400 px cap.

See [`../../assets/image-diet`](../../assets/image-diet/) for all original/result pairs.

## Use

1. Make FFmpeg available in `PATH`, or put `ffmpeg.exe` next to the program files.
2. Double-click `TASY-Image-Diet.bat`.
3. Drag PNG/JPG/JPEG files into the window, or select files/folders.
4. Choose the destination folder.
5. Optionally enable deletion of originals after successful processing.
6. Press **ОБРАБОТАТЬ**.

## Technical details

The current tested version uses FFmpeg for resize, JPEG encoding and metadata stripping. Longest side is capped at 1400 px and images are never enlarged. Output is JPEG.

**Transparency warning:** PNG transparency is not preserved because the output is JPEG. Keep originals when alpha or lossless data matters.

## Files

- [`TASY-Image-Diet.bat`](TASY-Image-Diet.bat) — launcher
- [`TASY-Image-Diet.ps1`](TASY-Image-Diet.ps1) — PowerShell / WinForms source

FFmpeg is not bundled. See the repository [THIRD_PARTY.md](../../THIRD_PARTY.md).
