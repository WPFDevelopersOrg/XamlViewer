using System;
using System.Runtime.InteropServices;

namespace XamlUtil.Common
{
    public static class Win32
    {
        public const int WS_SHOWNORMAL = 1;
        public const int WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct CopyData
        {
            public IntPtr dwData;
            public int cbData;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow = WS_SHOWNORMAL);

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref CopyData lParam);

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
