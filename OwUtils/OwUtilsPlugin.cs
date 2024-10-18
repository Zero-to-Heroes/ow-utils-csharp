using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public void copyImageDataUrlToClipboard(string dataUrl, Action<object, object> callback)
        {

            Thread t = new Thread((ThreadStart)(() =>
            {
                Logger.Log = onGlobalEvent;
                var base64Data = dataUrl.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                using (var ms = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(ms);
                    Clipboard.SetImage(image);
                }
                callback?.Invoke(null, null);
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

        public void deleteFileOrFolder(string path, Action<object, object> callback)
        {
            Task.Run(() =>
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    ClearFolder(path);
                    Directory.Delete(path);
                }
                callback?.Invoke(null, null);
            });
        }

        public void copyFiles(string sourceDir, string targetDir, Action<object, object> callback)
        {
            Task.Run(() =>
            {
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
                }
                callback?.Invoke(null, null);
            });
        }

        public void copyFile(string sourcePath, string targetDir, Action<object, object> callback)
        {
            Task.Run(() =>
            {
                File.Copy(sourcePath, Path.Combine(targetDir, Path.GetFileName(sourcePath)), true);
                callback?.Invoke(null, null);
            });
        }

        public void downloadAndUnzipFile(string fileUrl, string installPath, Action<object, object> callback)
        {
            //callback?.Invoke(null, $"Starting to download {fileUrl} and install it to {installPath}");
            //Thread t = new Thread((ThreadStart)(() =>
            //{
            //callback?.Invoke(null, $"Running Task");
            Task.Run(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        //callback?.Invoke(null, $"Created web client");
                        var fileName = $"MelonLoader-{new Random().NextDouble()}.zip";
                        var zipPath = Path.Combine(Path.GetTempPath(), fileName);
                        //callback?.Invoke(null, $"Built zipPath {zipPath}");
                        client.DownloadFile(fileUrl, zipPath);
                        //callback?.Invoke(null, $"Downloaded file");
                        var zipArchive = ZipFile.OpenRead(zipPath);
                        ExtractToDirectory(zipArchive, installPath);
                        //callback?.Invoke(null, $"Extracted file");
                    }
                    callback?.Invoke(true, null);
                }
                catch (Exception e)
                {
                    callback?.Invoke(null, e.Message);
                }
            });
            //}));

            //t.SetApartmentState(ApartmentState.STA);
            //t.Start();
            //t.Join();
        }

        public void downloadFileTo(string fileUrl, string installPath, string targetFileName, Action<object, object> callback)
        {
            Task.Run(() =>
            {
                int retriesLeft = 2;
                while (retriesLeft >= 0)
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            var targetPath = Path.Combine(installPath, targetFileName);
                            client.DownloadFile(fileUrl, targetPath);
                        }
                        callback?.Invoke(true, null);
                        return;
                    }
                    catch (Exception e)
                    {
                        retriesLeft--;
                        continue;
                    }
                }
                callback?.Invoke(null, null);
            });
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

        private void ClearFolder(string directoryPath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(directoryPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName)
        {
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
