using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class CalendarSyncScene : IScene
    {
        public string Name => "Calendar";

        public static string ICalUrl { get; set; } = "";

        private class CalendarEvent
        {
            public string Summary { get; set; } = "Meeting";
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        private readonly List<CalendarEvent> _events = new List<CalendarEvent>();
        private CalendarEvent? _nextEvent;
        
        private bool _isLoaded = false;
        private DateTime _lastFetch = DateTime.MinValue;
        
        private readonly HttpClient _httpClient = new HttpClient();

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // Background
            Color bgColor = Color.FromRgb(15, 10, 10);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = bgColor;
                }
            }

            // If no URL is set, show settings notice
            if (string.IsNullOrEmpty(ICalUrl))
            {
                PixelFont.DrawText(canvas, "PASTE", 6, 4, Color.FromRgb(255, 128, 0));
                PixelFont.DrawText(canvas, "ICAL URL", 1, 12, Color.FromRgb(255, 128, 0));
                PixelFont.DrawText(canvas, "IN APP", 4, 20, Color.FromRgb(255, 128, 0));
                return canvas;
            }

            // Fetch in background every 10 minutes
            if (!_isLoaded || (DateTime.Now - _lastFetch).TotalMinutes > 10)
            {
                _lastFetch = DateTime.Now;
                Task.Run(FetchCalendarAsync);
            }

            if (!_isLoaded)
            {
                PixelFont.DrawText(canvas, "SYNCING", 2, 10, Color.FromRgb(0, 255, 255));
                PixelFont.DrawText(canvas, "CALENDAR", 0, 16, Color.FromRgb(0, 255, 255));
                return canvas;
            }

            // Find current next event
            UpdateNextEvent();

            // Draw Cute Pixel Calendar Icon in the center top (y=1 to y=15)
            DrawCalendarIcon(canvas, frameCount);

            // Draw meeting status text
            if (_nextEvent == null)
            {
                // No meetings left today
                PixelFont.DrawText(canvas, "NO EVENTS", 4, 21, Color.FromRgb(128, 128, 128));
                PixelFont.DrawText(canvas, "TODAY", 7, 27, Color.FromRgb(128, 128, 128));
            }
            else
            {
                var now = DateTime.Now;
                string statusText = "";
                Color textColor = Color.FromRgb(255, 255, 255);

                if (now >= _nextEvent.StartTime && now <= _nextEvent.EndTime)
                {
                    // Happening now!
                    int timeLeft = (int)Math.Ceiling((_nextEvent.EndTime - now).TotalMinutes);
                    statusText = $"NOW: {_nextEvent.Summary} ({timeLeft}M LEFT)  ~  ";
                    textColor = Color.FromRgb(255, 50, 50); // Red alert color
                }
                else
                {
                    // Upcoming
                    double mins = (_nextEvent.StartTime - now).TotalMinutes;
                    if (mins < 60)
                    {
                        statusText = $"IN {(int)mins}M: {_nextEvent.Summary}  ~  ";
                        textColor = Color.FromRgb(255, 165, 0); // Orange warning
                    }
                    else
                    {
                        int hours = (int)Math.Round(mins / 60.0);
                        statusText = $"IN {hours}H: {_nextEvent.Summary}  ~  ";
                        textColor = Color.FromRgb(30, 215, 96); // Green standard
                    }
                }

                // Scroll status text at y=23
                int textWidth = PixelFont.MeasureTextWidth(statusText);
                int scrollRange = textWidth + 8;
                int textX = 32 - (frameCount % scrollRange);
                PixelFont.DrawText(canvas, statusText, textX, 23, textColor);
            }

            return canvas;
        }

        private void DrawCalendarIcon(Color[,] canvas, int frameCount)
        {
            Color border = Color.FromRgb(200, 200, 200);
            Color redHeader = Color.FromRgb(230, 50, 50);
            Color sheetColor = Color.FromRgb(255, 255, 255);

            // Draws a 12x12 calendar page at x=10, y=4
            // Top red header
            for (int y = 4; y <= 6; y++)
            {
                for (int x = 10; x <= 21; x++)
                {
                    canvas[y, x] = redHeader;
                }
            }

            // White paper sheet
            for (int y = 7; y <= 15; y++)
            {
                for (int x = 10; x <= 21; x++)
                {
                    canvas[y, x] = sheetColor;
                }
            }

            // Rings / Binder coils at top
            canvas[3, 12] = border; canvas[4, 12] = Color.FromRgb(0, 0, 0);
            canvas[3, 15] = border; canvas[4, 15] = Color.FromRgb(0, 0, 0);
            canvas[3, 18] = border; canvas[4, 18] = Color.FromRgb(0, 0, 0);

            // Draw a little grid pattern on the calendar sheet to look like days
            canvas[9, 12] = border; canvas[9, 15] = border; canvas[9, 18] = border;
            canvas[12, 12] = border; canvas[12, 15] = border; canvas[12, 18] = border;
        }

        private void UpdateNextEvent()
        {
            var now = DateTime.Now;
            _nextEvent = null;

            lock (_events)
            {
                foreach (var ev in _events)
                {
                    // Check if event is happening now, or starting in the future
                    if (ev.EndTime > now)
                    {
                        if (_nextEvent == null || ev.StartTime < _nextEvent.StartTime)
                        {
                            _nextEvent = ev;
                        }
                    }
                }
            }
        }

        private async Task FetchCalendarAsync()
        {
            if (string.IsNullOrEmpty(ICalUrl)) return;

            try
            {
                string icsText = await _httpClient.GetStringAsync(ICalUrl);
                var parsed = ParseICal(icsText);

                lock (_events)
                {
                    _events.Clear();
                    _events.AddRange(parsed);
                }
                _isLoaded = true;
            }
            catch (Exception)
            {
                // Ignore connection errors, reuse old events
            }
        }

        private List<CalendarEvent> ParseICal(string icsText)
        {
            var list = new List<CalendarEvent>();
            
            // Normalize line endings
            string[] lines = icsText.Replace("\r", "").Split('\n');
            
            CalendarEvent? currentEvent = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    currentEvent = new CalendarEvent();
                }
                else if (line.StartsWith("END:VEVENT", StringComparison.OrdinalIgnoreCase) && currentEvent != null)
                {
                    // Store only events from today onwards
                    if (currentEvent.EndTime > DateTime.Now.Date)
                    {
                        list.Add(currentEvent);
                    }
                    currentEvent = null;
                }
                else if (currentEvent != null)
                {
                    if (line.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEvent.Summary = line.Substring(8).Trim();
                    }
                    else if (line.StartsWith("DTSTART", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEvent.StartTime = ParseICalDate(line);
                    }
                    else if (line.StartsWith("DTEND", StringComparison.OrdinalIgnoreCase))
                    {
                        currentEvent.EndTime = ParseICalDate(line);
                    }
                }
            }

            return list;
        }

        private DateTime ParseICalDate(string line)
        {
            // Format of line is: DTSTART;TZID=Europe/London:20260626T123000 or DTSTART:20260626T123000Z
            int colonIdx = line.IndexOf(':');
            if (colonIdx == -1) return DateTime.Now;

            string dateVal = line.Substring(colonIdx + 1).Trim();
            
            try
            {
                // Format: 20260626T123000 or 20260626T123000Z
                string pattern = @"^(\d{4})(\d{2})(\d{2})T(\d{2})(\d{2})(\d{2})(Z)?";
                var match = Regex.Match(dateVal, pattern);
                if (match.Success)
                {
                    int yr = int.Parse(match.Groups[1].Value);
                    int mn = int.Parse(match.Groups[2].Value);
                    int dy = int.Parse(match.Groups[3].Value);
                    int hr = int.Parse(match.Groups[4].Value);
                    int mi = int.Parse(match.Groups[5].Value);
                    int sc = int.Parse(match.Groups[6].Value);
                    bool isUtc = match.Groups[7].Success;

                    DateTime dt = new DateTime(yr, mn, dy, hr, mi, sc, isUtc ? DateTimeKind.Utc : DateTimeKind.Local);
                    if (isUtc)
                    {
                        return dt.ToLocalTime();
                    }
                    return dt;
                }
            }
            catch { }

            return DateTime.Now;
        }

        public void Stop() { }
    }
}
