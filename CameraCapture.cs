using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace idotmatrix_gui
{
    public class CameraCapture
    {
        public static CameraCapture Instance { get; } = new CameraCapture();

        private MediaCapture? _mediaCapture;
        private bool _isInitialized = false;
        private VideoFrame? _videoFrame;

        private CameraCapture() { }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;

            try
            {
                _mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };
                
                await _mediaCapture.InitializeAsync(settings);
                
                // Pre-allocate a 32x32 BGRA8 VideoFrame
                _videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, 32, 32);
                
                _isInitialized = true;
                return true;
            }
            catch (Exception)
            {
                CleanUp();
                return false;
            }
        }

        public async Task<Color[,]> CaptureFrameAsync()
        {
            Color[,] pixels = new Color[32, 32];
            
            // Fill with black by default
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    pixels[y, x] = Color.FromRgb(0, 0, 0);

            if (!_isInitialized || _mediaCapture == null || _videoFrame == null)
            {
                bool success = await InitializeAsync();
                if (!success) return pixels;
            }

            try
            {
                // Grab the preview frame resized to 32x32 directly in BGRA8 format
                var frame = await _mediaCapture!.GetPreviewFrameAsync(_videoFrame);
                if (frame != null && frame.SoftwareBitmap != null)
                {
                    using (var buffer = frame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
                    {
                        using (var reference = buffer.CreateReference())
                        {
                            unsafe
                            {
                                byte* dataInBytes;
                                uint capacity;
                                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                                // Copy the 32x32 BGRA8 pixel bytes (stride is 32 * 4 = 128 bytes)
                                int stride = 128;
                                for (int y = 0; y < 32; y++)
                                {
                                    for (int x = 0; x < 32; x++)
                                    {
                                        int idx = y * stride + x * 4;
                                        byte b = dataInBytes[idx];
                                        byte g = dataInBytes[idx + 1];
                                        byte r = dataInBytes[idx + 2];
                                        
                                        // Simple vibrancy enhancement: boost contrast/saturation slightly
                                        r = ApplyVibrancy(r);
                                        g = ApplyVibrancy(g);
                                        b = ApplyVibrancy(b);

                                        pixels[y, x] = Color.FromRgb(r, g, b);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Handle camera disconnection/error
            }

            return pixels;
        }

        private byte ApplyVibrancy(byte val)
        {
            // Simple S-curve contrast boost
            double v = val / 255.0;
            v = 1.0 / (1.0 + Math.Exp(-10.0 * (v - 0.5))); // Sigmoid contrast
            return (byte)Math.Max(0, Math.Min(255, v * 255.0));
        }

        public void CleanUp()
        {
            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
            if (_videoFrame != null)
            {
                _videoFrame.Dispose();
                _videoFrame = null;
            }
            _isInitialized = false;
        }

        // Native interface to access direct byte arrays from WinRT IMemoryBuffer
        [ComImport]
        [Guid("5B0D3235-4DBE-4DFA-824C-3B3D9AE61C49")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMemoryBufferByteAccess
        {
            unsafe void GetBuffer(out byte* buffer, out uint capacity);
        }
    }
}
