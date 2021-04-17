using System;
using System.Runtime.InteropServices;

namespace XamlUtil.Common
{
    public static class Win32
    {
        public const int WS_SHOWNORMAL = 1;

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow = WS_SHOWNORMAL);

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    }
}
