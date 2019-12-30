using System;
using System.Windows;
using System.Windows.Threading;

namespace XamlTheme.Utils
{
    public static class WindowUtil
    {
        //deal black border while SizeToContent.WidthAndHeight
        public static void HandleSizeToContent(Window win, Action action = null)
        {
            if (win.SizeToContent == SizeToContent.Manual)
                return;

            var previosTopXPosition = win.Left;
            var previosTopYPosition = win.Top;
            var previosWidth = win.RestoreBounds.Width;
            var previosHeight = win.RestoreBounds.Height;

            var previousWindowStartupLocation = win.WindowStartupLocation;
            var previousSizeToContent = win.SizeToContent;

            win.SizeToContent = SizeToContent.Manual;
            win.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() =>
            {
                win.SizeToContent = previousSizeToContent;

                win.WindowStartupLocation = WindowStartupLocation.Manual;

                win.Left = previosTopXPosition + (previosWidth - win.ActualWidth) / 2;
                win.Top = previosTopYPosition + (previosHeight - win.ActualHeight) / 2;
                win.WindowStartupLocation = previousWindowStartupLocation;

                if (action != null)
                    action();
            }));
        } 
    }
}
