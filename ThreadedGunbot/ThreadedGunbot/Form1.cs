using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;

namespace ThreadedGunbot
{
    public partial class Form1 : Form
    {
        // Constants for the event flags
        public const uint KEYEVENTF_KEYDOWN = 0x0000;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // Virtual key codes for A, D, S, W, X
        public const ushort VK_A = 0x41;
        public const ushort VK_D = 0x44;
        public const ushort VK_S = 0x53;
        public const ushort VK_W = 0x57;
        public const ushort VK_X = 0x58;

        // Import SendInput from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        // Define the structures needed for SendInput
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        // Function to simulate a key press and release
        public void SendKeyPress(ushort keyCode)
        {
            INPUT inputDown = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            INPUT inputUp = new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // Send the key down event
            SendInput(1, ref inputDown, Marshal.SizeOf(typeof(INPUT)));

            // Send the key up event
            SendInput(1, ref inputUp, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(1);

        }

        // Function to simulate a left mouse click (button down and up)
        public void ClickLeftMouseButton()
        {
            // Mouse down event
            INPUT inputDown = new INPUT
            {
                type = 0, // Mouse input
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTDOWN,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // Mouse up event
            INPUT inputUp = new INPUT
            {
                type = 0, // Mouse input
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            Thread.Sleep(2);
            // Send the mouse down event
            SendInput(1, ref inputDown, Marshal.SizeOf(typeof(INPUT)));

            // Send the mouse up event
            SendInput(1, ref inputUp, Marshal.SizeOf(typeof(INPUT)));


        }


    





        private ConcurrentDictionary<int, string> cannonNeeds = new ConcurrentDictionary<int, string>();


        private ConcurrentDictionary<int, string> cannonData = new ConcurrentDictionary<int, string>();

        private ConcurrentDictionary<int, ColorType> loopColors = new ConcurrentDictionary<int, ColorType>();
        private ConcurrentDictionary<int, ColorType> entryColors = new ConcurrentDictionary<int, ColorType>(); 

        private ScreenshotCapture screenshotCapture;
        private IntPtr hWnd;
        private bool captureScreenshots = false; // Flag to control screenshot capturing

        // Import the necessary Windows API functions to find the window by title
        [DllImport("user32.dll")]
        public static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        // Define the RECT structure to hold window position
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        // Method to retrieve window coordinates
        public Point GetWindowPosition(IntPtr hWnd)
        {
            RECT windowRect = new RECT();
            // Call GetWindowRect to get the position of the window
            GetWindowRect(hWnd, ref windowRect);

            // The window's position is represented by the top-left corner of the window (Left, Top)
            return new Point(windowRect.Left, windowRect.Top);
        }
        // Define delegate for EnumWindows function
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Enum for color names
        public enum ColorType
        {
            Powder,
            Cloth,
            Ball,
            Water,
            Filled0,
            Filled1,
            Filled2,
            Filled3,
            Dirty0,
            Dirty1,
            Dirty2,
            Dirty3,
            Unknown
        }

        // Globalizing pixel positions and expected colors as LoopPixels
        private Point[] LoopPixels = new Point[]
        {
            new Point(35, 310), //Loop 0
            new Point(425, 310), //Loop1
            new Point(425, 380), //Loop 2
            new Point(35, 380), //Loop3
        };
                private Point[] EntryPixels = new Point[]
        {
            new Point(35, 250), 
            new Point(425, 250), 
            new Point(425, 440), 
            new Point(35, 440), 
        };


        private Point[] Cannon0Pixels = new Point[]
{
            new Point(135, 250),
            new Point(105, 250),
            new Point(75, 250),

};
        private Point[] Cannon1Pixels = new Point[]
{
            new Point(325, 250),
            new Point(355, 250),
            new Point(385, 250),

};
        private Point[] Cannon2Pixels = new Point[]
{
            new Point(325, 440),
            new Point(355, 440),
            new Point(385, 440),

};
        private Point[] Cannon3Pixels = new Point[]
{
            new Point(135, 440),
            new Point(105, 440),
            new Point(75, 440),

};





        // Expected colors using RGB values
        private Color[] expectedColors = new Color[]
        {
            Color.FromArgb(255,127,39),   // Powder
            Color.FromArgb(179,179,217),   // Cloth
            Color.FromArgb(64,0,64),   // Ball
            Color.FromArgb(128,255,255), // Water
            Color.FromArgb(234,0,0),//Filled0
            Color.FromArgb(112,1,1),//Filled1
            Color.FromArgb(112,1,3),//Filled2
            Color.FromArgb(222,0,0),//Filled3
            Color.FromArgb(123,121,114),//Dirty0
            Color.FromArgb(65,43,29),//Dirty1
            Color.FromArgb(55,54,57),//Dirty2
            Color.FromArgb(5,5,5),//Dirty3

        };
        private Color[] FillColors = new Color[]
        {
            Color.FromArgb(255,127,39),   // 
            Color.FromArgb(179,179,217),   // 
            Color.FromArgb(64,0,64),   // 
            Color.FromArgb(128,255,255), //
        };

        public Form1()
        {
            InitializeComponent();
            screenshotCapture = new ScreenshotCapture(); // Initialize ScreenshotCapture
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Find the window handle based on partial window title
            hWnd = FindWindowWithPartialTitle("Puzzle Pirates -");

            if (hWnd != IntPtr.Zero)
            {
                captureScreenshots = true;
                Task.Run(() => CaptureAndUpdateScreenshots());
                Task.Run(() => LoopAnalyzer());
                Task.Run(() => EntryAnalyzer());
                Thread.Sleep(100);
                Task.Run(() => Cannon0Analyzer());
                Thread.Sleep(100);
                Task.Run(() => Cannon1Analyzer());
                Thread.Sleep(100);
                Task.Run(() => Cannon2Analyzer());
                Thread.Sleep(100);
                Task.Run(() => Cannon3Analyzer());
                Thread.Sleep(100);
                Task.Run(() => Solver());
            }
            else
            {
                MessageBox.Show("Window not found!");
            }
        }


        private void CaptureAndUpdateScreenshots()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Update the PictureBox on the UI thread
                    Invoke(new Action(() =>
                    {
                        // Dispose the old image before setting the new one
                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                        }
                        DrawCircles(screenshot);
                        pictureBox1.Image = screenshot;
                    }));
                }
            }
        }
        private void UpdateCannonNeed(int cannonPosition, string need)
        {
            // Try to get the current need for the cannon position
            if (!cannonNeeds.TryGetValue(cannonPosition, out string currentNeed))
            {
                // If the key doesn't exist, set the default value
                cannonNeeds[cannonPosition] = "Unknown";
            }

            // Update the cannon need in the dictionary
            cannonNeeds[cannonPosition] = need;
        }
        private IntPtr FindWindowWithPartialTitle(string partialTitle)
        {
            IntPtr foundWindow = IntPtr.Zero;

            // Define a callback function for EnumWindows
            EnumWindowsProc enumProc = new EnumWindowsProc((hWnd, lParam) =>
            {
                // Get the window title
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

                // Check if the title contains the partial title
                if (windowTitle.ToString().Contains(partialTitle))
                {
                    foundWindow = hWnd;
                    return false; // Stop enumerating windows
                }

                return true; // Continue enumerating
            });

            // Enumerate all windows
            EnumWindows(enumProc, IntPtr.Zero);

            return foundWindow;
        }

        private void LoopAnalyzer()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Check each pixel and store the corresponding color type for positions 0, 1, 2, 3
                    for (int i = 0; i < LoopPixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(LoopPixels[i].X, LoopPixels[i].Y);

                        // Determine the color type for the current pixel position
                        ColorType colorType = GetColorType(pixelColor);

                        // Store the result in the global dictionary
                        loopColors[i] = colorType;

                        // Log the current pixel color for debugging
                        //Debug.WriteLine($"Loop {i} at {LoopPixels[i]}: Pixel color: {pixelColor} (ColorType: {colorType})");
                    }

                    screenshot.Dispose();
                }
            }
        }
        private void EntryAnalyzer()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Check each pixel and store the corresponding color type for positions 0, 1, 2, 3
                    for (int i = 0; i < EntryPixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(EntryPixels[i].X, EntryPixels[i].Y);
                        ColorType colorType = GetColorType(pixelColor);

                        // Store the color type in the entryColors dictionary
                        entryColors[i] = colorType;

                        // Optionally, log the color type for debugging
                        switch (colorType)
                        {
                            case ColorType.Powder:
                                //Debug.WriteLine($"Entry {i}: Powder");
                                break;
                            case ColorType.Cloth:
                                //Debug.WriteLine($"Entry {i}: Cloth");
                                break;
                            case ColorType.Ball:
                                //Debug.WriteLine($"Entry {i}: Ball");
                                break;
                            case ColorType.Water:
                                //Debug.WriteLine($"Entry {i}: Water");
                                break;
                            default:
                                //Debug.WriteLine($"Entry {i}: Unknown color");
                                break;
                        }
                    }

                    // Dispose of the screenshot after use
                    screenshot.Dispose();
                }
            }
        }

        private void Cannon0Analyzer()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Variables to hold the overall need for the cannon
                    string overallNeed = "";

                    // Flags to track the states of positions
                    string position0 = "";
                    string position1 = "";
                    string position2 = "";

                    // Analyze each pixel for cannon 0 and check their color types
                    for (int i = 0; i < Cannon0Pixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(Cannon0Pixels[i].X, Cannon0Pixels[i].Y);
                        ColorType colorType = GetColorType(pixelColor);

                        // Log the pixel color for each position
                        //Debug.WriteLine($"Cannon 0 - Position {i}: Pixel color: {pixelColor} (ColorType: {colorType})");

                        // Analyze the positions and track the need for the cannon overall
                        switch (i)
                        {
                            case 0: // Position 0
                                if (colorType == ColorType.Powder)
                                {
                                    position0 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position0 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position0 = "Ball";
                                }
                                else if (colorType == ColorType.Dirty0)
                                {
                                    position0 = "Dirty";
                                }
                                else
                                {
                                    position0 = "Empty";
                                }
                                break;

                            case 1: // Position 1
                                if (colorType == ColorType.Powder)
                                {
                                    position1 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position1 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position1 = "Ball";
                                }
                                else
                                {
                                    position1 = "Empty";
                                }
                                break;

                            case 2: // Position 2
                                if (colorType == ColorType.Powder)
                                {
                                    position2 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position2 = "Cloth";
                                }
                                else if (colorType == ColorType.Filled0)
                                {
                                    position2 = "Filled";  // Special case for Filled0
                                }
                                else
                                {
                                    position2 = "Empty";
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    // Debugging the positions
                   // Debug.WriteLine("Checking With positions:");
                   // Debug.WriteLine($"position0: {position0}");
                   // Debug.WriteLine($"position1: {position1}");
                   // Debug.WriteLine($"position2: {position2}");

                    // Step 1: Check Position 2 first (highest priority)
                    if (position2 == "Filled")
                    {
                        overallNeed = "Filled";  // If Position 2 has Filled0, no need required
                    }

                    else
                    {
                        // Step 2: Check Position 0 and Position 1
                        if (position0 == "Powder")
                        {
                            // If Position 0 needs Powder, check Position 1
                            if (position1 == "Cloth")
                            {
                                overallNeed = "Ball";  // If Position 1 has Cloth, it needs Ball
                            }
                            else if (position1 == "Empty")
                            {
                                overallNeed = "Cloth";  // If Position 1 is Empty, it needs Cloth
                            }
                            else if (position1 == "Ball" || position1 == "Powder")
                            {
                                overallNeed = "Water";  // If Position 1 has Ball or Powder, it needs Water
                            }
                            else
                            {
                                overallNeed = "Water";
                            }
                        }
                        else if (position0 == "Empty")
                        {
                            overallNeed = "Powder";  // If Position 0 is Empty, it needs Powder
                        }
                        else
                        {
                            overallNeed = "Water";  // Default case: if Position 0 is not Powder, it needs Water
                        }
                        if (position2 == "Cloth" || position2 == "Powder")
                        {
                            overallNeed = "Water";
                        }
                    }

                    // Debugging the final decision
                    //Debug.WriteLine($"Final Cannon Need: {overallNeed}");

                    // Update the final need based on the decision
                    UpdateCannonNeed(0, overallNeed);

                    screenshot.Dispose();
                }
            }
        }






        private void Cannon1Analyzer()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Variables to hold the overall need for the cannon
                    string overallNeed = "";

                    // Flags to track the states of positions
                    string position0 = "";
                    string position1 = "";
                    string position2 = "";

                    // Analyze each pixel for cannon 1 and check their color types
                    for (int i = 0; i < Cannon1Pixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(Cannon1Pixels[i].X, Cannon1Pixels[i].Y);
                        ColorType colorType = GetColorType(pixelColor);

                        // Log the pixel color for each position
                        //Debug.WriteLine($"Cannon 1 - Position {i}: Pixel color: {pixelColor} (ColorType: {colorType})");

                        // Analyze the positions and track the need for the cannon overall
                        switch (i)
                        {
                            case 0: // Position 0
                                if (colorType == ColorType.Powder)
                                {
                                    position0 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position0 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position0 = "Ball";
                                }
                                else if (colorType == ColorType.Dirty1)
                                {
                                    position0 = "Dirty";
                                }
                                else
                                {
                                    position0 = "Empty";
                                }
                                break;

                            case 1: // Position 1
                                if (colorType == ColorType.Powder)
                                {
                                    position1 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position1 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position1 = "Ball";
                                }
                                else
                                {
                                    position1 = "Empty";
                                }
                                break;

                            case 2: // Position 2
                                if (colorType == ColorType.Powder)
                                {
                                    position2 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position2 = "Cloth";
                                }
                                else if (colorType == ColorType.Filled1)
                                {
                                    position2 = "Filled";  // Special case for Filled0
                                }
                                else
                                {
                                    position2 = "Empty";
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    // Debugging the positions
                    //Debug.WriteLine("Checking With positions:");
                   // Debug.WriteLine($"position0: {position0}");
                    //Debug.WriteLine($"position1: {position1}");
                    //Debug.WriteLine($"position2: {position2}");

                    // Step 1: Check Position 2 first (highest priority)
                    if (position2 == "Filled")
                    {
                        overallNeed = "Filled";  // If Position 2 has Filled0, no need required
                    }

                    else
                    {
                        // Step 2: Check Position 0 and Position 1
                        if (position0 == "Powder")
                        {
                            // If Position 0 needs Powder, check Position 1
                            if (position1 == "Cloth")
                            {
                                overallNeed = "Ball";  // If Position 1 has Cloth, it needs Ball
                            }
                            else if (position1 == "Empty")
                            {
                                overallNeed = "Cloth";  // If Position 1 is Empty, it needs Cloth
                            }
                            else if (position1 == "Ball" || position1 == "Powder")
                            {
                                overallNeed = "Water";  // If Position 1 has Ball or Powder, it needs Water
                            }
                        }
                        else if (position0 == "Empty")
                        {
                            overallNeed = "Powder";  // If Position 0 is Empty, it needs Powder
                        }
                        else
                        {
                            overallNeed = "Water";  // Default case: if Position 0 is not Powder, it needs Water
                        }

                        if (position2 == "Cloth" || position2 == "Powder")
                        {
                            overallNeed = "Water";
                        }
                    }

                    // Debugging the final decision
                    //Debug.WriteLine($"Final Cannon Need: {overallNeed}");

                    // Update the final need based on the decision
                    UpdateCannonNeed(1, overallNeed);

                    screenshot.Dispose();
                }
            }
        }

        private void Cannon2Analyzer()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Variables to hold the overall need for the cannon
                    string overallNeed = "";

                    // Flags to track the states of positions
                    string position0 = "";
                    string position1 = "";
                    string position2 = "";

                    // Analyze each pixel for cannon 2 and check their color types
                    for (int i = 0; i < Cannon2Pixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(Cannon2Pixels[i].X, Cannon2Pixels[i].Y);
                        ColorType colorType = GetColorType(pixelColor);

                        // Log the pixel color for each position
                       //Debug.WriteLine($"Cannon 2 - Position {i}: Pixel color: {pixelColor} (ColorType: {colorType})");

                        // Analyze the positions and track the need for the cannon overall
                        switch (i)
                        {
                            case 0: // Position 0
                                if (colorType == ColorType.Powder)
                                {
                                    position0 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position0 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position0 = "Ball";
                                }
                                else if (colorType == ColorType.Dirty2)
                                {
                                    position0 = "Dirty";
                                }
                                else
                                {
                                    position0 = "Empty";
                                }
                                break;

                            case 1: // Position 1
                                if (colorType == ColorType.Powder)
                                {
                                    position1 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position1 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position1 = "Ball";
                                }
                                else
                                {
                                    position1 = "Empty";
                                }
                                break;

                            case 2: // Position 2
                                if (colorType == ColorType.Powder)
                                {
                                    position2 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position2 = "Cloth";
                                }
                                else if (colorType == ColorType.Filled1)
                                {
                                    position2 = "Filled";  // Special case for Filled0
                                }
                                else
                                {
                                    position2 = "Empty";
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    // Debugging the positions
                   // Debug.WriteLine("Checking With positions:");
                  //  Debug.WriteLine($"position0: {position0}");
                   // Debug.WriteLine($"position1: {position1}");
                  // Debug.WriteLine($"position2: {position2}");

                    // Step 1: Check Position 2 first (highest priority)
                    if (position2 == "Filled")
                    {
                        overallNeed = "Filled";  // If Position 2 has Filled0, no need required
                    }

                    else
                    {
                        // Step 2: Check Position 0 and Position 1
                        if (position0 == "Powder")
                        {
                            // If Position 0 needs Powder, check Position 1
                            if (position1 == "Cloth")
                            {
                                overallNeed = "Ball";  // If Position 1 has Cloth, it needs Ball
                            }
                            else if (position1 == "Empty")
                            {
                                overallNeed = "Cloth";  // If Position 1 is Empty, it needs Cloth
                            }
                            else if (position1 == "Ball" || position1 == "Powder")
                            {
                                overallNeed = "Water";  // If Position 1 has Ball or Powder, it needs Water
                            }
                        }
                        else if (position0 == "Empty")
                        {
                            overallNeed = "Powder";  // If Position 0 is Empty, it needs Powder
                        }
                        else
                        {
                            overallNeed = "Water";  // Default case: if Position 0 is not Powder, it needs Water
                        }

                        if (position2 == "Cloth" || position2 == "Powder")
                        {
                            overallNeed = "Water";
                        }
                    }

                    // Debugging the final decision
                    //Debug.WriteLine($"Final Cannon Need: {overallNeed}");

                    // Update the final need based on the decision
                    UpdateCannonNeed(2, overallNeed);

                    screenshot.Dispose();
                }
            }
        }

        private void Cannon3Analyzer()
        {
            while (captureScreenshots)
            {
                // Capture the screenshot of the window
                Bitmap screenshot = screenshotCapture.CaptureScreenshot(hWnd);

                if (screenshot != null)
                {
                    // Variables to hold the overall need for the cannon
                    string overallNeed = "";

                    // Flags to track the states of positions
                    string position0 = "";
                    string position1 = "";
                    string position2 = "";

                    // Analyze each pixel for cannon 3 and check their color types
                    for (int i = 0; i < Cannon3Pixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(Cannon3Pixels[i].X, Cannon3Pixels[i].Y);
                        ColorType colorType = GetColorType(pixelColor);

                        // Log the pixel color for each position
                        //Debug.WriteLine($"Cannon 3 - Position {i}: Pixel color: {pixelColor} (ColorType: {colorType})");

                        // Analyze the positions and track the need for the cannon overall
                        switch (i)
                        {
                            case 0: // Position 0
                                if (colorType == ColorType.Powder)
                                {
                                    position0 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position0 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position0 = "Ball";
                                }
                                else if (colorType == ColorType.Dirty3)
                                {
                                    position0 = "Dirty";
                                }
                                else
                                {
                                    position0 = "Empty";
                                }
                                break;

                            case 1: // Position 1
                                if (colorType == ColorType.Powder)
                                {
                                    position1 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position1 = "Cloth";
                                }
                                else if (colorType == ColorType.Ball)
                                {
                                    position1 = "Ball";
                                }
                                else
                                {
                                    position1 = "Empty";
                                }
                                break;

                            case 2: // Position 2
                                if (colorType == ColorType.Powder)
                                {
                                    position2 = "Powder";
                                }
                                else if (colorType == ColorType.Cloth)
                                {
                                    position2 = "Cloth";
                                }
                                else if (colorType == ColorType.Filled3)
                                {
                                    position2 = "Filled";  // Special case for Filled0
                                }
                                else
                                {
                                    position2 = "Empty";
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    // Debugging the positions
                    //Debug.WriteLine("Checking With positions:");
                    //Debug.WriteLine($"position0: {position0}");
                    //Debug.WriteLine($"position1: {position1}");
                   // Debug.WriteLine($"position2: {position2}");

                    // Step 1: Check Position 2 first (highest priority)
                    if (position2 == "Filled")
                    {
                        overallNeed = "Filled";  // If Position 2 has Filled0, no need required
                    }

                    else
                    {
                        // Step 2: Check Position 0 and Position 1
                        if (position0 == "Powder")
                        {
                            // If Position 0 needs Powder, check Position 1
                            if (position1 == "Cloth")
                            {
                                overallNeed = "Ball";  // If Position 1 has Cloth, it needs Ball
                            }
                            else if (position1 == "Empty")
                            {
                                overallNeed = "Cloth";  // If Position 1 is Empty, it needs Cloth
                            }
                            else if (position1 == "Ball" || position1 == "Powder")
                            {
                                overallNeed = "Water";  // If Position 1 has Ball or Powder, it needs Water
                            }
                        }
                        else if (position0 == "Empty")
                        {
                            overallNeed = "Powder";  // If Position 0 is Empty, it needs Powder
                        }
                        else
                        {
                            overallNeed = "Water";  // Default case: if Position 0 is not Powder, it needs Water
                        }

                        if (position2 == "Cloth" || position2 == "Powder")
                        {
                            overallNeed = "Water";
                        }
                    }

                    // Debugging the final decision
                   // Debug.WriteLine($"Final Cannon Need: {overallNeed}");

                    // Update the final need based on the decision
                    UpdateCannonNeed(3, overallNeed);

                    screenshot.Dispose();
                }
            }
        }

        private ColorType GetColorType(Color color)
        {
            for (int i = 0; i < expectedColors.Length; i++)
            {
                // Log both the actual color and the expected color for debugging
                //Debug.WriteLine($"Comparing actual color: {color} with expected color: {expectedColors[i]}");

                // Perform exact color match
                if (color == expectedColors[i])
                {
                    return (ColorType)i; // Return the corresponding color type
                }
            }

            // If no match is found, return a default color type
            return ColorType.Unknown;
        }

        private void DrawCircles(Bitmap screenshot)
        {
            // Create a Graphics object to draw on the screenshot
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                // Define a pen to draw the circles (e.g., Red color with 2px width)
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    // Draw a circle around each LoopPixel position
                    foreach (Point pixel in LoopPixels)
                    {
                        g.DrawEllipse(pen, pixel.X - 5, pixel.Y - 5, 10, 10); // Circle of radius 5
                    }
                }

                using (Pen pen = new Pen(Color.Blue, 2))
                {
                    // Draw a circle around each LoopPixel position
                    foreach (Point pixel in EntryPixels)
                    {
                        g.DrawEllipse(pen, pixel.X - 5, pixel.Y - 5, 10, 10); // Circle of radius 5
                    }
                }
                using (Pen pen = new Pen(Color.Green, 2))
                {
                    // Draw a circle around each LoopPixel position
                    foreach (Point pixel in Cannon0Pixels)
                    {
                        g.DrawEllipse(pen, pixel.X - 5, pixel.Y - 5, 10, 10); // Circle of radius 5
                    }
                }
                using (Pen pen = new Pen(Color.Green, 2))
                {
                    // Draw a circle around each LoopPixel position
                    foreach (Point pixel in Cannon1Pixels)
                    {
                        g.DrawEllipse(pen, pixel.X - 5, pixel.Y - 5, 10, 10); // Circle of radius 5
                    }
                }
                using (Pen pen = new Pen(Color.Green, 2))
                {
                    // Draw a circle around each LoopPixel position
                    foreach (Point pixel in Cannon2Pixels)
                    {
                        g.DrawEllipse(pen, pixel.X - 5, pixel.Y - 5, 10, 10); // Circle of radius 5
                    }
                }
                using (Pen pen = new Pen(Color.Green, 2))
                {
                    // Draw a circle around each LoopPixel position
                    foreach (Point pixel in Cannon3Pixels)
                    {
                        g.DrawEllipse(pen, pixel.X - 5, pixel.Y - 5, 10, 10); // Circle of radius 5
                    }
                }

            }

            // Update the PictureBox to show the new screenshot with circles
            Invoke(new Action(() =>
            {
                // Dispose of the old image before setting the new one
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                pictureBox1.Image = screenshot; // Set the updated image with circles
            }));
        }
        private void Solver()
        {
            while (true)
            {
                SolveCannonNeeds();
            }
        }
        private void SolveCannonNeeds()
        {
            // Iterate over each cannon position and log its needs
            for (int i = 0; i < 4; i++)  // We have 4 cannons (0 to 3)
            {
                string need = "Unknown";

                // Try to get the cannon need from the dictionary, if not found use "Unknown"
                if (cannonNeeds.TryGetValue(i, out need))
                {
                    Debug.WriteLine($"Cannon {i}: Needs {need}");

                    // Get the loop color for the respective cannon from the loopColors dictionary
                    ColorType loopColorType;
                    if (loopColors.TryGetValue(i, out loopColorType))
                    {
                        Debug.WriteLine($"Cannon {i} - Loop {i} (ColorType: {loopColorType})");

                        // Now perform actions based on the cannon need and the color of the respective loop position
                        // Cannon 0 actions
                        if (i == 0 && need == "Powder" && loopColorType == ColorType.Powder)
                        {
                            PerformCannon0Action(ColorType.Powder); // Perform Cannon 0's specific action
                        }
                        else if (i == 0 && need == "Ball" && loopColorType == ColorType.Ball)
                        {
                            PerformCannon0Action(ColorType.Ball); // Perform Cannon 0's specific action
                        }
                        else if (i == 0 && need == "Water" && loopColorType == ColorType.Water)
                        {
                            PerformCannon0Action(ColorType.Water); // Perform Cannon 0's specific action
                        }

                        // Cannon 1 actions
                        else if (i == 1 && need == "Powder" && loopColorType == ColorType.Powder)
                        {
                            PerformCannon1Action(ColorType.Powder); // Perform Cannon 1's specific action
                        }
                        else if (i == 1 && need == "Ball" && loopColorType == ColorType.Ball)
                        {
                            PerformCannon1Action(ColorType.Ball); // Perform Cannon 1's specific action
                        }
                        else if (i == 1 && need == "Water" && loopColorType == ColorType.Water)
                        {
                            PerformCannon1Action(ColorType.Water); // Perform Cannon 1's specific action
                        }

                        // Cannon 2 actions
                        else if (i == 2 && need == "Powder" && loopColorType == ColorType.Powder)
                        {
                            PerformCannon2Action(ColorType.Powder); // Perform Cannon 2's specific action
                        }
                        else if (i == 2 && need == "Ball" && loopColorType == ColorType.Ball)
                        {
                            PerformCannon2Action(ColorType.Ball); // Perform Cannon 2's specific action
                        }
                        else if (i == 2 && need == "Water" && loopColorType == ColorType.Water)
                        {
                            PerformCannon2Action(ColorType.Water); // Perform Cannon 2's specific action
                        }

                        // Cannon 3 actions
                        else if (i == 3 && need == "Powder" && loopColorType == ColorType.Powder)
                        {
                            PerformCannon3Action(ColorType.Powder); // Perform Cannon 3's specific action
                        }
                        else if (i == 3 && need == "Ball" && loopColorType == ColorType.Ball)
                        {
                            PerformCannon3Action(ColorType.Ball); // Perform Cannon 3's specific action
                        }
                        else if (i == 3 && need == "Water" && loopColorType == ColorType.Water)
                        {
                            PerformCannon3Action(ColorType.Water); // Perform Cannon 3's specific action
                        }

                        else
                        {
                            Debug.WriteLine($"Cannon {i}: Condition not met, no action performed.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Cannon {i}: Loop color data not available.");
                    }
                }
                else
                {
                    Debug.WriteLine($"Cannon {i}: No need found, defaulting to Unknown.");
                }
            }
        }




        private async void PerformCannon0Action(ColorType desiredColor)
        {
            // Step 1: Perform initial actions (move mouse, press keys)
            MoveMouseToWindowWithOffset(hWnd, LoopPixels[0].X, LoopPixels[0].Y);
            SendKeyPress(VK_W);
            ClickLeftMouseButton();
            SendKeyPress(VK_S);
            ClickLeftMouseButton();

            MoveMouseToWindowWithOffset(hWnd, EntryPixels[0].X, EntryPixels[0].Y);
            SendKeyPress(VK_D);


            bool colorMatched = await WaitForDesiredColorAsync(desiredColor, 200);
            if (colorMatched)
            {
                ClickLeftMouseButton(); // Example of action when color is found
            }
            SendKeyPress(VK_A);
            SendKeyPress(VK_X);
            FixLoop();
        }

        private async void PerformCannon1Action(ColorType desiredColor)
        {
            // Step 1: Perform initial actions (move mouse, press keys)
            MoveMouseToWindowWithOffset(hWnd, LoopPixels[1].X, LoopPixels[1].Y);
            SendKeyPress(VK_W);
            ClickLeftMouseButton();
            SendKeyPress(VK_S);
            ClickLeftMouseButton();
            FixLoop();
            MoveMouseToWindowWithOffset(hWnd, EntryPixels[1].X, EntryPixels[1].Y);
            SendKeyPress(VK_A);


            bool colorMatched = await WaitForDesiredColorAsync(desiredColor, 200);
            if (colorMatched)
            {
                ClickLeftMouseButton(); // Example of action when color is found
            }
            SendKeyPress(VK_D);
            ClickLeftMouseButton();
            FixLoop();
        }

        private async void PerformCannon2Action(ColorType desiredColor)
        {
            // Step 1: Perform initial actions (move mouse, press keys)
            MoveMouseToWindowWithOffset(hWnd, LoopPixels[2].X, LoopPixels[2].Y);
            SendKeyPress(VK_S);
            ClickLeftMouseButton();
            SendKeyPress(VK_W);
            SendKeyPress(VK_X);
            FixLoop();
            MoveMouseToWindowWithOffset(hWnd, EntryPixels[2].X, EntryPixels[2].Y);
            SendKeyPress(VK_A);


            bool colorMatched = await WaitForDesiredColorAsync(desiredColor, 200);
            if (colorMatched)
            {
                ClickLeftMouseButton();
            }
            SendKeyPress(VK_D);
            ClickLeftMouseButton();
            FixLoop();
        }

        private async void PerformCannon3Action(ColorType desiredColor)
        {
            // Step 1: Perform initial actions (move mouse, press keys)
            MoveMouseToWindowWithOffset(hWnd, LoopPixels[3].X, LoopPixels[3].Y);
            SendKeyPress(VK_S);
            ClickLeftMouseButton();
            SendKeyPress(VK_W);
            ClickLeftMouseButton();
            FixLoop();
            MoveMouseToWindowWithOffset(hWnd, EntryPixels[3].X, EntryPixels[3].Y);
            SendKeyPress(VK_D);


            bool colorMatched = await WaitForDesiredColorAsync(desiredColor, 200);
            if (colorMatched)
            {
                ClickLeftMouseButton();
            }
            SendKeyPress(VK_A);
            ClickLeftMouseButton();
            FixLoop();
        }

        private void FixLoop()
        {
            MoveMouseToWindowWithOffset(hWnd, LoopPixels[0].X, LoopPixels[0].Y);
            SendKeyPress(VK_D);
            ClickLeftMouseButton();
            MoveMouseToWindowWithOffset(hWnd, LoopPixels[2].X, LoopPixels[2].Y);
            SendKeyPress(VK_A);
            ClickLeftMouseButton();
        }

        private async Task<bool> WaitForDesiredColorAsync(ColorType desiredColor, int timeoutMs)
        {
            DateTime startTime = DateTime.Now;

            // Poll every 10ms to check if the desired color is found within the timeout
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                // Check if the color of EntryPixels[0] matches the desired color
                if (entryColors.TryGetValue(0, out ColorType currentColor) && currentColor == desiredColor)
                {
                    return true; // Desired color matched
                }
            }

            return false; // Desired color not found within the timeout
        }

        // You can use this method to move the mouse
        public void MoveMouseToWindowWithOffset(IntPtr hWnd, int offsetX, int offsetY)
        {
            // Get the window position
            Point windowPos = GetWindowPosition(hWnd);

            // Apply the offset
            int targetX = windowPos.X + offsetX;
            int targetY = windowPos.Y + offsetY;

            // Move the mouse to the desired position
            SetCursorPos(targetX, targetY); // SetCursorPos sets the mouse position on the screen
            Thread.Sleep(1);
        }
    }
}
