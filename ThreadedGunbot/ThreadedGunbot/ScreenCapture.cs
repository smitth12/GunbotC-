using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ThreadedGunbot
{
    public class ScreenshotCapture
    {
        // Import necessary Windows API functions
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Declare necessary structs
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Method to capture a screenshot of the window based on handle
        public Bitmap CaptureScreenshot(IntPtr hWnd)
        {
            // Get the dimensions of the window
            RECT rect;
            if (GetWindowRect(hWnd, out rect))
            {
                // Capture the screenshot of the window
                return CaptureWindow(hWnd, rect);
            }
            else
            {
                throw new InvalidOperationException("Unable to get window dimensions.");
            }
        }

        // Method to capture a screenshot of the window
        private Bitmap CaptureWindow(IntPtr hWnd, RECT rect)
        {
            // Create a bitmap to store the screenshot
            Bitmap bmp = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);

            // Create a Graphics object to capture the screen
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(bmp.Width, bmp.Height));
            }

            return bmp;
        }
    }
}
