using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace OwUtils
{
    public class OwUtilsPlugin
    {
        // a global event that triggers with two parameters:
        //
        // plugin.get().onGlobalEvent.addListener(function(first, second) {
        //  ...
        // });
        public event Action<object, object> onGlobalEvent;

        public void captureWindow(string windowName, string destinationFolder, bool copyToClipBoard, Action<object, object> callback)
        {
            Thread t = new Thread((ThreadStart)(() =>
            {
                Logger.Log = onGlobalEvent;
                var image = windowName == null || windowName.Length == 0
                    ? ScreenCapture.CaptureActiveWindow(copyToClipBoard)
                    : ScreenCapture.CaptureWindow(windowName, copyToClipBoard);
                //Logger.Log("captured screenshot", "");
                if (image == null)
                {
                    callback(null, null);
                    return;
                }
                var base64Image = ImageToBase64String(image);
                //Logger.Log("converted image to base64", base64Image);
                var destination = $"{destinationFolder}/firestone_screenshot.jpg";
                image.Save(destination, ImageFormat.Jpeg);
                //Logger.Log("Saved image", destination);
                callback(destination, base64Image);
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }


        public void flashWindow(string windowName, Action<object, object> callback)
        {
            Thread t = new Thread((ThreadStart)(() =>
            {
                Logger.Log = onGlobalEvent;
                FlashWindow.Flash(windowName);
                callback?.Invoke(null, null);
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        public string ImageToBase64String(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                var newImage = new Bitmap(imageIn);
                newImage.Save(ms, ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}
