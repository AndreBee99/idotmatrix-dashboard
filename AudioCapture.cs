using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;

namespace idotmatrix_gui
{
    public class AudioCapture
    {
        public static AudioCapture Instance { get; } = new AudioCapture();

        private WasapiCapture? _capture;
        private readonly int _fftLength = 1024;
        private readonly int _m = 10; // 2^10 = 1024
        private readonly float[] _hanningWindow;
        private readonly Complex[] _fftBuffer;
        
        private readonly object _lock = new object();
        private readonly List<float> _sampleBuffer = new List<float>();

        public double CurrentBassFactor { get; private set; } = 0;
        public float[] FftResults { get; private set; }

        private double _bassRunningMax = 0.1;
        private readonly double _bassAgcDecay = 0.995;

        private AudioCapture()
        {
            FftResults = new float[_fftLength / 2];
            _fftBuffer = new Complex[_fftLength];
            
            // Precompute Hanning window
            _hanningWindow = new float[_fftLength];
            for (int i = 0; i < _fftLength; i++)
            {
                _hanningWindow[i] = (float)(0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (_fftLength - 1))));
            }
        }

        public List<MMDevice> GetAudioDevices(bool includeLoopback)
        {
            var devices = new List<MMDevice>();
            var enumerator = new MMDeviceEnumerator();
            
            if (includeLoopback)
            {
                // Speakers for loopback
                var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                devices.AddRange(renderDevices);
            }
            else
            {
                // Microphones
                var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                devices.AddRange(captureDevices);
            }
            return devices;
        }

        public void Start(MMDevice? device, bool useLoopback)
        {
            Stop();

            try
            {
                var enumerator = new MMDeviceEnumerator();
                var targetDevice = device ?? enumerator.GetDefaultAudioEndpoint(
                    useLoopback ? DataFlow.Render : DataFlow.Capture, 
                    Role.Multimedia
                );

                if (useLoopback)
                {
                    _capture = new WasapiLoopbackCapture(targetDevice);
                }
                else
                {
                    _capture = new WasapiCapture(targetDevice);
                }

                _capture.DataAvailable += OnDataAvailable;
                _capture.StartRecording();
            }
            catch (Exception)
            {
                // Handle or log error
            }
        }

        public void Stop()
        {
            if (_capture != null)
            {
                _capture.DataAvailable -= OnDataAvailable;
                _capture.StopRecording();
                _capture.Dispose();
                _capture = null;
            }
            
            lock (_lock)
            {
                _sampleBuffer.Clear();
            }
            CurrentBassFactor = 0;
            Array.Clear(FftResults, 0, FftResults.Length);
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_capture == null) return;

            int bytesPerSample = _capture.WaveFormat.BitsPerSample / 8;
            int channels = _capture.WaveFormat.Channels;
            
            lock (_lock)
            {
                for (int i = 0; i < e.BytesRecorded; i += bytesPerSample * channels)
                {
                    float floatVal = 0f;
                    if (bytesPerSample == 4) // Float
                    {
                        floatVal = BitConverter.ToSingle(e.Buffer, i);
                    }
                    else if (bytesPerSample == 2) // 16-bit Int
                    {
                        short shortVal = BitConverter.ToInt16(e.Buffer, i);
                        floatVal = shortVal / 32768f;
                    }

                    // Store mono/left channel sample
                    _sampleBuffer.Add(floatVal);
                }

                // Process FFT once we have enough samples
                while (_sampleBuffer.Count >= _fftLength)
                {
                    ProcessFft();
                    
                    // Shift buffer (50% overlap for smoother visuals)
                    _sampleBuffer.RemoveRange(0, _fftLength / 2);
                }
            }
        }

        private void ProcessFft()
        {
            // 1. Fill FFT buffer with Hanning windowed samples
            for (int i = 0; i < _fftLength; i++)
            {
                _fftBuffer[i].X = _sampleBuffer[i] * _hanningWindow[i]; // Real part
                _fftBuffer[i].Y = 0f;                                   // Imaginary part
            }

            // 2. Perform FFT
            FastFourierTransform.FFT(true, _m, _fftBuffer);

            // 3. Compute Magnitudes for the first half (Nyquist limit)
            int halfLen = _fftLength / 2;
            float[] tempResults = new float[halfLen];
            for (int i = 0; i < halfLen; i++)
            {
                float real = _fftBuffer[i].X;
                float imag = _fftBuffer[i].Y;
                tempResults[i] = (float)Math.Sqrt(real * real + imag * imag);
            }
            
            FftResults = tempResults;

            // 4. Extract Bass (Bins 1 to 4 correspond to ~40Hz to ~170Hz at 44.1kHz)
            double rawBass = 0;
            for (int i = 1; i <= 4; i++)
            {
                rawBass += FftResults[i];
            }
            rawBass /= 4.0;

            // Update AGC (Automatic Gain Control)
            if (rawBass > _bassRunningMax)
            {
                _bassRunningMax = rawBass;
            }
            else
            {
                _bassRunningMax = Math.Max(0.001, _bassRunningMax * _bassAgcDecay);
            }

            double normalizedBass = rawBass / _bassRunningMax;

            // Smooth decay
            if (normalizedBass > CurrentBassFactor)
            {
                CurrentBassFactor = normalizedBass;
            }
            else
            {
                CurrentBassFactor = CurrentBassFactor * 0.75 + normalizedBass * 0.25;
            }
        }
    }
}
