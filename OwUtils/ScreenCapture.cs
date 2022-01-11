using System;
using System.Runtime.InteropServices;
using System.Drawing;
using HWND = System.IntPtr;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace OwUtils
{
    public class ScreenCapture
    {
        public static Image CaptureDesktop()
        {
            return CaptureWindow(WindowUtils.GetDesktopWindow(), false);
        }

        public static Bitmap CaptureActiveWindow(bool copyToClipBoard)
        {
            return CaptureWindow(WindowUtils.GetForegroundWindow(), copyToClipBoard);
        }

        public static Bitmap CaptureWindow(string windowName, bool copyToClipBoard)
        {
            foreach (KeyValuePair<IntPtr, string> window in WindowUtils.GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;

                Console.WriteLine("{0}: {1}", handle, title);
                if (title.Equals(windowName))
                {
                    return CaptureWindow(handle, copyToClipBoard);

                }
            }
            return null;
        }

        public static Bitmap CaptureWindow(IntPtr handle, bool copyToClipBoard)
        {
            var rect = new WindowUtils.Rect();
            WindowUtils.GetWindowRect(handle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            if (copyToClipBoard)
            {
                Clipboard.SetImage(result);
            }

            return result;
        }
    }
}
