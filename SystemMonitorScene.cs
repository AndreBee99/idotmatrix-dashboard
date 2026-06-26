using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Collections.Generic;

namespace idotmatrix_gui
{
    public class SystemMonitorScene : IScene
    {
        public string Name => "System Monitor";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _diskCounter;
        private readonly List<PerformanceCounter> _gpuCounters = new List<PerformanceCounter>();

        private bool _cpuFailed = false;
        private bool _gpuInitialized = false;
        private bool _gpuFailed = false;
        private bool _diskFailed = false;

        private float _cpuLoad = 0;
        private float _ramLoad = 0;
        private float _gpuLoad = 0;
        private float _diskLoad = 0;

        private DateTime _lastUpdate = DateTime.MinValue;

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // Background (Sleek dark void)
            Color bgColor = Color.FromRgb(5, 5, 8);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = bgColor;
                }
            }

            // Update performance stats once every second
            if ((DateTime.Now - _lastUpdate).TotalMilliseconds > 1000)
            {
                UpdateStats();
                _lastUpdate = DateTime.Now;
            }

            // Draw grid separator lines
            Color gridColor = Color.FromRgb(25, 25, 35);
            for (int x = 0; x < 32; x++) canvas[16, x] = gridColor;
            for (int y = 0; y < 32; y++) canvas[y, 16] = gridColor;

            // Draw 4 gauges
            // 1. Top-Left: CPU (Cyan)
            DrawGauge(canvas, 7, 7, _cpuLoad, "C", Color.FromRgb(0, 240, 255));

            // 2. Top-Right: GPU (Magenta)
            DrawGauge(canvas, 24, 7, _gpuLoad, "G", Color.FromRgb(255, 0, 128));

            // 3. Bottom-Left: RAM (Neon Green)
            DrawGauge(canvas, 7, 24, _ramLoad, "R", Color.FromRgb(30, 215, 96));

            // 4. Bottom-Right: Disk (Orange/Amber)
            DrawGauge(canvas, 24, 24, _diskLoad, "D", Color.FromRgb(255, 128, 0));

            return canvas;
        }

        private void DrawGauge(Color[,] canvas, int cx, int cy, float value, string label, Color themeColor)
        {
            double rArc = 6.0;
            double rNeedle = 5.0;
            Color needleColor = Color.FromRgb(255, 255, 255); // Opaque white needle
            Color redlineColor = Color.FromRgb(255, 0, 0); // Redline color

            // 1. Draw the dial arc (from -45 to 225 degrees)
            for (double a = -45; a <= 225; a += 1.5)
            {
                double rad = a * Math.PI / 180.0;
                int px = (int)Math.Round(cx + rArc * Math.Cos(rad));
                int py = (int)Math.Round(cy - rArc * Math.Sin(rad));

                if (px >= 0 && px < 32 && py >= 0 && py < 32)
                {
                    // Value > 80% represents redline zone (which is angle < 225 - 0.8*270 = 9 degrees)
                    if (a < 9)
                    {
                        canvas[py, px] = redlineColor;
                    }
                    else
                    {
                        canvas[py, px] = themeColor;
                    }
                }
            }

            // 2. Draw the needle
            double valueAngle = 225.0 - (Math.Min(100.0, Math.Max(0.0, value)) / 100.0) * 270.0;
            double valueRad = valueAngle * Math.PI / 180.0;

            for (double d = 0; d <= rNeedle; d += 0.5)
            {
                int px = (int)Math.Round(cx + d * Math.Cos(valueRad));
                int py = (int)Math.Round(cy - d * Math.Sin(valueRad));
                if (px >= 0 && px < 32 && py >= 0 && py < 32)
                {
                    canvas[py, px] = needleColor;
                }
            }

            // 3. Draw the center pivot point (dark center hub overlay)
            if (cx >= 0 && cx < 32 && cy >= 0 && cy < 32)
            {
                canvas[cy, cx] = Color.FromRgb(40, 40, 45);
            }

            // 4. Draw the label centered at bottom (3x5 character)
            PixelFont.DrawText(canvas, label, cx - 1, cy + 3, themeColor);
        }

        private void UpdateStats()
        {
            // 1. Fetch RAM Load via GlobalMemoryStatusEx
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                _ramLoad = memStatus.dwMemoryLoad;
            }
            else
            {
                _ramLoad = 0;
            }

            // 2. Fetch CPU Load via PerformanceCounter
            if (!_cpuFailed)
            {
                try
                {
                    if (_cpuCounter == null)
                    {
                        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        _cpuCounter.NextValue();
                    }
                    _cpuLoad = _cpuCounter.NextValue();
                }
                catch
                {
                    _cpuFailed = true;
                    _cpuCounter = null;
                }
            }

            if (_cpuFailed)
            {
                Random r = new Random();
                _cpuLoad = Math.Max(5, Math.Min(95, _cpuLoad + r.Next(-8, 9)));
            }

            // 3. Fetch GPU Load via Performance Counters (GPU Engine 3D)
            if (!_gpuFailed)
            {
                try
                {
                    if (!_gpuInitialized)
                    {
                        InitializeGpuCounters();
                    }

                    float totalGpu = 0;
                    foreach (var counter in _gpuCounters)
                    {
                        totalGpu += counter.NextValue();
                    }
                    _gpuLoad = Math.Min(100f, totalGpu);
                }
                catch
                {
                    _gpuFailed = true;
                }
            }

            if (_gpuFailed)
            {
                // Fallback simulation proportional to CPU load with variance
                Random r = new Random();
                _gpuLoad = Math.Max(0, Math.Min(100, (_cpuLoad * 0.7f) + r.Next(-10, 11)));
            }

            // 4. Fetch Disk Load (% Disk Time)
            if (!_diskFailed)
            {
                try
                {
                    if (_diskCounter == null)
                    {
                        _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                        _diskCounter.NextValue();
                    }
                    _diskLoad = Math.Min(100f, _diskCounter.NextValue());
                }
                catch
                {
                    _diskFailed = true;
                    _diskCounter = null;
                }
            }

            if (_diskFailed)
            {
                Random r = new Random();
                _diskLoad = Math.Max(2, Math.Min(98, _diskLoad + r.Next(-15, 16)));
            }
        }

        private void InitializeGpuCounters()
        {
            if (_gpuInitialized || _gpuFailed) return;
            try
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                string[] instances = category.GetInstanceNames();
                _gpuCounters.Clear();
                foreach (string instance in instances)
                {
                    // Focus on 3D rendering engines to measure main GPU workload
                    if (instance.Contains("3D", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var counter in category.GetCounters(instance))
                        {
                            if (counter.CounterName == "Utilization Percentage")
                            {
                                counter.NextValue();
                                _gpuCounters.Add(counter);
                            }
                        }
                    }
                }
                _gpuInitialized = true;
            }
            catch
            {
                _gpuFailed = true;
            }
        }

        public void Stop()
        {
            _cpuCounter?.Dispose();
            _cpuCounter = null;

            _diskCounter?.Dispose();
            _diskCounter = null;

            foreach (var counter in _gpuCounters)
            {
                counter.Dispose();
            }
            _gpuCounters.Clear();
            _gpuInitialized = false;
        }
    }
}
