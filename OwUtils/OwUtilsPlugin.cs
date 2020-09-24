using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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

        public void captureWindow(string windowName, string destinationFolder, Action<object, object> callback)
        {
            Logger.Log = onGlobalEvent; 
            var image = windowName == null || windowName.Length == 0 
                ? ScreenCapture.CaptureActiveWindow() 
                : ScreenCapture.CaptureWindow(windowName);
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
