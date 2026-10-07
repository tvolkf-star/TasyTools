# TASY Water Motion

**Animate water in a still image. No neural networks. No heavyweight software.**

Open or drag an image into the window, paint the areas of water that should move, choose the motion and preview the result.

- **Left mouse:** paint water
- **Right mouse:** erase
- Adjustable brush: **10–500 px**
- Water presets: **CALM · RIPPLE · WIND**
- Adjustable motion strength and duration
- Optional **CAMERA IN** movement
- Seamless procedural loop
- H.264 MP4 export at **30 fps**
- Source image is never modified

The mask is not only a boundary. It is part of the motion control. Selective, irregular painting can break up overly uniform movement and make reflections feel more natural.

## Build

Run `BUILD-EXE.bat`. It uses the classic .NET Framework C# compiler used by the other C# TasyTools.

## FFmpeg

FFmpeg is not bundled. Water Motion looks for `tools\\ffmpeg.exe`, then `ffmpeg.exe` beside the program, then system `PATH`.

Copyright © 2026 **Tasy Volkova**
