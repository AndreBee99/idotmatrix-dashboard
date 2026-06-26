# iDotMatrix WPF DIY Dashboard

A high-performance, neon-themed WPF desktop companion application for the **iDotMatrix 32x32 LED Panel** (e.g., `IDM-XXXXXX`).

The application runs in the background (with Windows system tray integration) and connects to your LED display over Bluetooth LE to stream custom visual modules, real-time system stats, and live notifications at a fluid 20 FPS.

---

## What the App Does

*   **Real-Time Notification Interceptor**: Instantly overrides the display to show clean, customized pixel-art alerts (Slack, Discord, Outlook, Spotify, WhatsApp, etc.) with pulsing neon alert borders.
*   **Dynamic Scene Carousel**: Automatically cycles through a customizable playlist of visual modules with adjustable timings and retro screen distortion transition shaders (VHS tracking glitch, Paint Melt, and Sine Waves).
*   **Background Operation & Persistence**: Minimizes to the system tray, auto-connects to your screen, and saves all settings (weather location, MAC address, calendar feeds) to a local configuration file.

---

## Visual Scenes Overview

*   **Music Media Sync**: Displays active track metadata and album art from Windows Media Session (SMTC), with a **32-band WASAPI audio spectrum visualizer** that pulses in sync with your system's bass frequencies.
*   **Analog Instrument Monitor**: Visualizes CPU, GPU, RAM, and Disk load as **4 sports car dashboard dials** with real-time needle sweeps and redline indicators for high loads.
*   **Weather & Calendar Forecasts**: Pulls Open-Meteo weather data to display animated weather icons, and syncs private/public iCal feeds to display meeting countdowns.
*   **Webcam Mirror**: Captures low-latency webcam feeds with real-time contrast and vibrancy enhancements.
*   **Bouncing DVD Logo**: A nostalgic screensaver scene that changes colors on wall bounces and flashes when hitting corners.

---

## Getting Started

### Prerequisites

*   Windows 10 (Version 1903+) or Windows 11 (Supports x64 and ARM64).
*   .NET 8.0 SDK (to compile from source).
*   Bluetooth-enabled hardware.

### Compiling and Running

1.  Clone the repository:
    ```bash
    git clone https://github.com/yourusername/idotmatrix-dashboard.git
    cd idotmatrix-dashboard
    ```
2.  Build and run the project using the dotnet CLI:
    ```bash
    # Run the application
    dotnet run -c Release
    ```
3.  Alternatively, publish a self-contained single file binary:
    ```bash
    dotnet publish -c Release -r win-x64 --self-contained true
    ```

### Connecting to Your Device

1.  Power on your iDotMatrix display (ensure it is disconnected from your mobile application).
2.  Turn on Bluetooth on your PC.
3.  Obtain your screen's BLE MAC Address (format: `XX:XX:XX:XX:XX:XX`). You can find this in your phone's app settings or by checking your system's Bluetooth devices.
4.  Input the address in the **MAC:** input box in the top-right corner of the Dashboard.
5.  Click **CONNECT**. The indicator will turn green when connected, and the terminal log will begin streaming BLE packets.
6.  *On first startup, Windows will prompt you to authorize notification listener access. Click **Allow** to enable the toast notification intercepts.*

---

## Credits & Attributions

This project relies on, interfaces with, or was inspired by the following open-source projects, datasets, and APIs:

*   **iDotMatrix BLE Protocol**: Reverse-engineered protocol specifications derived from the [python3-idotmatrix-library](https://github.com/derkalle4/python3-idotmatrix-library) by `derkalle4` and its various forks/discussions.
*   **Audio Capture & Analysis**: High-performance WASAPI audio loopback and spectrum calculation powered by [NAudio](https://github.com/naudio/NAudio).
*   **Weather Forecasts**: Geocoding and weather data courtesy of the free, non-commercial [Open-Meteo API](https://open-meteo.com/).
*   **Pokémon Sprites**: Live pixel-art animations dynamically loaded using the [PokéAPI](https://pokeapi.co/).
*   **Notification and Media Session Integration**: Integrated using Windows WinRT/UWP API contracts (`UserNotificationListener` and `GlobalSystemMediaTransportControlsSessionManager`).

---

## AI Vibe Code Disclaimer

> [!NOTE]
> **This project is 100% AI-generated vibe code.**
> It was crafted in a collaborative pair-programming journey with **Antigravity**, an agentic AI coding assistant designed by the Google DeepMind team. The entire architecture—including custom C# Bluetooth LE writing, real-time audio FFT, WinRT event hooks, retro HLSL-style screen distortion shaders, UI layouts, and tray integration—was brainstormed, coded, and iterated by AI. Enjoy the vibes!

---

## License

This project is licensed under the MIT License.

