using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ThreadedGunbot
{
    public partial class Form1 : Form
    {
        private ConcurrentDictionary<int, string> cannonNeeds = new ConcurrentDictionary<int, string>();


        private ConcurrentDictionary<int, string> cannonData = new ConcurrentDictionary<int, string>();



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

        // Declare necessary structs
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
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
                // Start the background task to capture screenshots and analyze pixels
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
                    // Check each pixel and log the corresponding color type for positions 0, 1, 2, 3
                    for (int i = 0; i < LoopPixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(LoopPixels[i].X, LoopPixels[i].Y);

                        // Log the current pixel color
                        
                        //Debug.WriteLine($"Pixel at position {i} (coordinates {LoopPixels[i]}) has color: {pixelColor}");

                        // Determine the color type for the current pixel position
                        ColorType colorType = GetColorType(pixelColor);

                        // Check if it matches any of the expected color types
                        switch (colorType)
                        {
                            case ColorType.Powder:
                                //Debug.WriteLine($"Loop {i}: Powder");
                                break;
                            case ColorType.Cloth:
                                //Debug.WriteLine($"Loop {i}: Cloth");
                                break;
                            case ColorType.Ball:
                                //Debug.WriteLine($"Loop {i}: Ball");
                                break;
                            case ColorType.Water:
                                //Debug.WriteLine($"Loop {i}: Water");
                                break;
                            default:
                                //Debug.WriteLine($"Pixel at position {i} (coordinates {LoopPixels[i]}) does NOT match any expected color.");
                                break;
                        }
                    }
                    //Debug.WriteLine("-----------------------------");
                    // Explicitly dispose of the screenshot after use
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
                    // Check each pixel and log the corresponding color type for positions 0, 1, 2, 3
                    for (int i = 0; i < EntryPixels.Length; i++)
                    {
                        Color pixelColor = screenshot.GetPixel(EntryPixels[i].X, EntryPixels[i].Y);
                        ColorType colorType = GetColorType(pixelColor);

                        switch (colorType)
                        {
                            case ColorType.Powder:
                                Debug.WriteLine($"Entry {i}: Powder");
                                break;
                            case ColorType.Cloth:
                                Debug.WriteLine($"Entry {i}: Cloth");
                                break;
                            case ColorType.Ball:
                                Debug.WriteLine($"Entry {i}: Ball");
                                break;
                            case ColorType.Water:
                                Debug.WriteLine($"Entry {i}: Water");
                                break;
                            default:
                                //Debug.WriteLine($"Pixel at position {i} (coordinates {LoopPixels[i]}) does NOT match any expected color.");
                                break;
                        }
                    }
                    //Debug.WriteLine("-----------------------------");
                    // Explicitly dispose of the screenshot after use
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

                    // Optionally, you can add more complex logic here depending on other conditions.
                    // For example, if you want to check for specific conditions:
                    if (need == "Powder")
                    {
                    }
                    else if (need == "Cloth")
                    {
                    }
                    else if (need == "Ball")
                    {
                    }
                    else if (need == "Water")
                    {
                    }
                }
                else
                {
                    Debug.WriteLine($"Cannon {i}: No need found, defaulting to Unknown.");
                }
            }
        }


    }
}
