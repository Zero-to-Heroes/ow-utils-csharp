using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
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

        public void Init()
        {
            Logger.Log = (string1, string2) => onGlobalEvent(string1, string2);
        }

        public void getFileVersion(string filePath, Action<object> callback)
        {
            // Get the file version info
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

            // Access the version information
            string fileVersion = fileVersionInfo.FileVersion;
            callback(fileVersion);
        }

        public void grantFullAccess(string dirPath, Action<object> callback)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = Path.Combine(assemblyDir, "OwUtilsExe.exe");
            Logger.Log("[ow-utils] [plugin] granting full access", fileName);
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"grantAccess \"{dirPath}\"",
                UseShellExecute = true,
                WorkingDirectory = assemblyDir,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                //CreateNoWindow = true,
                Verb = "runas" // This triggers the UAC prompt
            };

            try
            {
                using (Process process = Process.Start(processInfo))
                {
                    Logger.Log("[ow-utils] [plugin] started process", "");
                    // Read standard output line by line
                    while (!process.StandardOutput.EndOfStream)
                    {
                        Logger.Log("[ow-utils] [plugin] waiting for process", "");
                        string line = process.StandardOutput.ReadLine();
                        Logger.Log("[ow-utils] Received line from exe", line);
                        if (line.StartsWith("RESULT:"))
                        {
                            // Parse the result message
                            string[] parts = line.Substring(7).Split('|'); // Remove "RESULT:" and split
                            if (parts.Length == 1)
                            {
                                callback(parts[0]);
                            }
                        }
                        else if (line.StartsWith("ERROR:"))
                        {
                            // Capture errors
                            callback(line.Substring(6)); // Remove "ERROR:"
                        }
                    }

                    Logger.Log("[ow-utils] [plugin] waiting for exit", "");
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("[ow-utils] [plugin] got exception", ex.Message);
                callback($"Failed to start process: {ex.Message}");
            }
        }

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
            Logger.Log("Starting file copy", $"{sourcePath}, {targetDir}");
            Task.Run(() =>
            {
                try
                {
                    Logger.Log("Starting file copy in task", "");
                    File.Copy(sourcePath, Path.Combine(targetDir, Path.GetFileName(sourcePath)), true);
                    Logger.Log("After file copy", "");
                    callback?.Invoke(null, null);
                }
                catch (Exception e)
                {
                    Logger.Log($"Got error: ${e.Message}", "");
                    callback?.Invoke(e.Message, e.StackTrace);
                }
            });
        }

        public string CopyFileSync(string sourcePath, string targetDir)
        {
            try
            {
                File.Copy(sourcePath, Path.Combine(targetDir, Path.GetFileName(sourcePath)), true);
                return "success";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Got error: ${e.Message}");
                return e.Message;
            }
        }

        public string GrantAccessSync(string path)
        {
            try
            {
                DirectorySecurity sec = Directory.GetAccessControl(path);
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                sec.AddAccessRule(new FileSystemAccessRule(
                    everyone, 
                    FileSystemRights.Modify | FileSystemRights.Synchronize, 
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, 
                    PropagationFlags.None, 
                    AccessControlType.Allow));
                Directory.SetAccessControl(path, sec);
                return "success";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Got error: ${e.Message}");
                return e.Message;
            }
        }

        public void ListFilesInDirectory(string sourcePath, Action<object> callback)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = new List<dynamic>();

                    // List files
                    foreach (string file in Directory.GetFiles(sourcePath, "*.*"))
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        var fileResult = new
                        {
                            Name = fileInfo.Name,
                            LastModified = fileInfo.LastWriteTime.ToString("o"),
                            Type = "file"
                        };
                        result.Add(fileResult);
                    }

                    // List directories
                    foreach (string directory in Directory.GetDirectories(sourcePath, "*"))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(directory);
                        var dirResult = new
                        {
                            Name = dirInfo.Name,
                            LastModified = dirInfo.LastWriteTime.ToString("o"),
                            Type = "directory"
                        };
                        result.Add(dirResult);
                    }

                    callback?.Invoke(JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    Logger.Log($"An error occurred: {ex.Message}", "");
                }
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
