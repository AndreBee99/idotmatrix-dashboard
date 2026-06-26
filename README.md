# iDotMatrix WPF DIY Dashboard

A high-performance, neon-themed C# WPF desktop dashboard for the **iDotMatrix 32x32 LED Panel** (e.g. `IDM-XXXXXX`). It consolidates media controllers, weather updates, meeting notifications, system diagnostics, and webcam feeds into a customizable, modular carousel cycling at a fluid 20 FPS over Bluetooth LE.

---

## Key Features

*   **Modular Scene Carousel**: Cycle through visual modules with custom durations, order, and toggle-able states. Retro screen distortion transition shaders (VHS tracking glitch, Paint Melt, and Sine Wave) apply dynamically between swaps.
*   **System Notification Interceptor**: Real-time intercept of Windows toast notifications (Slack, Discord, Outlook, Spotify, etc.) via UWP `UserNotificationListener`. Instantly interrupts the carousel to display:
    *   **Phase 1 (Large 16x16 icon)**: Displays a clean app logo in the center and scrolls the app name at the bottom in Cyan.
    *   **Phase 2 (Small 8x8 icon)**: Scales the logo to the corner, scrolls the notification title in Magenta at the top, and scrolls the message body in White at the bottom.
    *   **Pulsing Alert Border**: Neon border pulses intensity to grab attention.
    *   *Includes hand-crafted, high-contrast pixel-art fallbacks for Discord (Clyde), Spotify, Slack, Outlook, and WhatsApp to bypass unpackaged WinRT asset limitations.*
*   **Analog Instrument Resource Monitor**: Displays CPU, GPU (Direct3D engines), RAM, and active Disk Write time in a 2x2 grid as **4 sports car dashboard dials** with real-time needle sweeps and a redline indicator for loads exceeding 80%.
*   **Music Media Sync**: Subscribes to Windows System Media Transport Controls (SMTC) to downscale album art, sync track progression, scroll song details, and modulate a glowing status bar in sync with real-time soundcard bass frequencies.
*   **WASAPI Audio Visualizer**: 32-band spectrum analyzer capturing system audio loopback with peak decay indicators.
*   **Weather forecaster**: Pulls Open-Meteo forecaster data with customized animated weather icons (Sun, clouds, rain, snow, lightning).
*   **Calendarsync**: Reads private/public iCal feeds to display upcoming meeting countdowns.
*   **Webcam Mirror**: Captures low-latency 32x32 webcam feeds with vibrancy/contrast enhancement.
*   **Bouncing DVD Logo**: Bounces the classic logo changing colors on walls and flashing on corner hits.
*   **State Persistence**: Settings (playlists, weather location, calendar feed, and target MAC address) automatically save to `%APPDATA%\iDotMatrixDashboard\config.json`.
*   **System Tray Integration**: Can be minimized to the system tray, running in the background, with double-click restoration and right-click menu controls.

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

## License

This project is licensed under the MIT License.
