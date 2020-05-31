using System.Runtime.InteropServices;

namespace XamlTheme.Utils
{
    public static class Win32
    {
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT point);
    }
}
