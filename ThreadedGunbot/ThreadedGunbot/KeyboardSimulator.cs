using System.Runtime.InteropServices;
using System;

public class KeyboardSimulator
{
    // Constants for the event flags
    public const uint KEYEVENTF_KEYDOWN = 0x0000;
    public const uint KEYEVENTF_KEYUP = 0x0002;

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
    public struct INPUT
    {
        public uint type;
        public INPUTUNION inputUnion;
    }

    public struct INPUTUNION
    {
        public KEYBDINPUT ki;
    }

    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public uint dwExtraInfo;
    }

    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public uint dwExtraInfo;
    }

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
            inputUnion = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = keyCode,
                    wScan = 0,
                    dwFlags = KEYEVENTF_KEYDOWN,
                    time = 0,
                    dwExtraInfo = 0
                }
            }
        };

        INPUT inputUp = new INPUT
        {
            type = 1, // Keyboard input
            inputUnion = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = keyCode,
                    wScan = 0,
                    dwFlags = KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = 0
                }
            }
        };

        // Send the key down event
        SendInput(1, ref inputDown, Marshal.SizeOf(typeof(INPUT)));

        // Send the key up event
        SendInput(1, ref inputUp, Marshal.SizeOf(typeof(INPUT)));
    }

    // Function to simulate key presses for multiple keys (e.g., A, D, S, W, X)
    public void PerformActions()
    {
        // Perform A, D, S, W, X actions
        SendKeyPress(VK_A); // Simulate pressing A
        SendKeyPress(VK_D); // Simulate pressing D
        SendKeyPress(VK_S); // Simulate pressing S
        SendKeyPress(VK_W); // Simulate pressing W
        SendKeyPress(VK_X); // Simulate pressing X
    }
}
