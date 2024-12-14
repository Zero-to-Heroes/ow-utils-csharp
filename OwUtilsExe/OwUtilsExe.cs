using OwUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OwUtilsExe
{
    public class OwUtilsExe
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private const int ATTACH_PARENT_PROCESS = -1;

        static void Main(string[] args) 
        {
            FreeConsole();
            AttachConsole(ATTACH_PARENT_PROCESS);

            string command = args[0];
            string source = args[1];
            //string destination = args[2];

            switch (command)
            {
                case "grantAccess":
                    var output1 = new OwUtilsPlugin().GrantAccessSync(source);
                    Console.WriteLine($"RESULT:{output1}");
                    break;
                //case "copyFile":
                //    var output2 = new OwUtilsPlugin().CopyFileSync(source, destination);
                //    Console.WriteLine($"RESULT:{output2}");
                //    break;
            }
        }
    }
}
