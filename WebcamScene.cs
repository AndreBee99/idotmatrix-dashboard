using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class WebcamScene : IScene
    {
        public string Name => "Webcam Mirror";

        private Color[,] _lastFrame = new Color[32, 32];
        private bool _isCapturing = false;

        public Color[,] DrawFrame(int frameCount)
        {
            if (!_isCapturing)
            {
                _isCapturing = true;
                // Capture the frame in a background task so it doesn't block the main rendering loop
                Task.Run(async () =>
                {
                    try
                    {
                        var frame = await CameraCapture.Instance.CaptureFrameAsync();
                        lock (_lastFrame)
                        {
                            _lastFrame = frame;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore errors during capture
                    }
                    finally
                    {
                        _isCapturing = false;
                    }
                });
            }

            lock (_lastFrame)
            {
                Color[,] frameCopy = new Color[32, 32];
                Array.Copy(_lastFrame, frameCopy, _lastFrame.Length);
                return frameCopy;
            }
        }

        public void Stop()
        {
            CameraCapture.Instance.CleanUp();
        }
    }
}
