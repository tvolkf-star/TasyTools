# TASY Water Motion · prototype

Experimental branch for animating water in a still image without changing the rest of the frame.

## Prototype v0

1. Open an image.
2. Paint the water mask with the left mouse button. Right mouse button erases.
3. Choose CALM / RIPPLE / WIND and strength.
4. Preview the seamless procedural loop.
5. Export H.264 MP4.

The first prototype deliberately does one job: horizontal multi-frequency displacement inside a hand-painted mask. Motion is periodic in time, so the exported sequence returns mathematically to its starting state at the loop boundary.

The source image is never modified.

### Build

Run `BUILD-EXE.bat`. It uses the classic .NET Framework C# compiler already used by the other C# TasyTools.

### FFmpeg

FFmpeg is not bundled. The prototype looks for `tools\\ffmpeg.exe`, then `ffmpeg.exe` beside the program, then system `PATH`.

This is a development prototype. It is intentionally kept off `main` until the water motion is tested on real architectural renders.
