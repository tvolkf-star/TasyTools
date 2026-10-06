# TASY Still Motion

**Create a short camera move from one still image.**

Still Motion is for architectural renders, illustrations, AI-generated images, concept art and other stills that need a little motion without opening a full video editor.

## What it does

1. Load an image.
2. Click **START**.
3. Click **FINISH**.
4. Choose duration, output size and movement style.
5. Export an H.264 MP4.

The program moves a fixed camera window across the larger source image. The source image is never modified.

## Controls

- duration: 5 / 10 / 15 / 20 seconds
- output: 480 FORUM / 1200 WEB / SOURCE
- movement: LOOK IN / LOOK UP / LOOK SIDE / DETAIL / DRIFT / LOOP
- output: MP4 H.264, CRF 18, `faststart`, `yuv420p`, 30 fps

LOOK UP and LOOK SIDE slightly bend the selected route. LOOP travels START → FINISH → START.

### Reading the output settings

- **480 FORUM**: long side capped at 480 px for small posts and previews.
- **1200 WEB**: long side capped at 1200 px for larger web presentation.
- **SOURCE**: uses the source-scale output path instead of a web-size cap.
- **5 / 10 / 15 / 20 sec**: finished clip duration.
- **CRF 18**: the H.264 quality target used by this tested version.
- **30 fps**: output frame rate.
- **faststart**: places MP4 playback metadata at the beginning for friendlier web playback.

## Real LOOP example

![Still Motion LOOP preview](../../assets/still-motion/loop-preview.webp)

**1200×1200 MP4 · LOOP · 15 s · 30 fps · 3.93 MB**

[Open the original MP4 example](../../assets/still-motion/loop-example.mp4)

The animated WebP above is a lightweight README preview derived from the MP4. The MP4 is the actual Still Motion output.

| | |
|---|---|
| ![Still Motion interface](../../assets/still-motion/ui-01.jpg) | ![Still Motion interface](../../assets/still-motion/ui-02.jpg) |
| ![Still Motion interface](../../assets/still-motion/ui-03.jpg) | ![Still Motion interface](../../assets/still-motion/ui-04.jpg) |

## FFmpeg lookup

The program searches in this order:

1. `tools\\ffmpeg.exe`
2. `ffmpeg.exe` beside the application
3. system `PATH`

## Build

Run [`BUILD-EXE.bat`](BUILD-EXE.bat). It uses the classic .NET Framework C# compiler from the standard Windows locations and creates `TASY-Still-Motion.exe` in the same folder. No IDE is required.

## Files

- [`TASY-Still-Motion.cs`](TASY-Still-Motion.cs) — C# / WinForms source
- [`BUILD-EXE.bat`](BUILD-EXE.bat) — Windows build script

FFmpeg is not bundled. See the repository [THIRD_PARTY.md](../../THIRD_PARTY.md).
