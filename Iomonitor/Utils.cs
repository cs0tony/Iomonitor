using Iomonitor.Interop;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Iomonitor
{
    internal class Utils
    {
        public static bool IsRunAtStartup()
        {
            bool result = false;
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                string rkValue = (string)rk.GetValue(appname, "");
                if (rkValue != "")
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public static RECT GetPrimaryScreenBounds()
        {
            return Screen.PrimaryScreen.Bounds;
        }

        public static bool IsCurrentdisplaySwitchModeExtend()
        {
            bool isExtend = false;
            // 获取屏幕数量
            int screenCount = Screen.AllScreens.Length;

            if (screenCount == 1)
            {
                isExtend = false;
            }
            else
            {
                bool isMirrored = false;
                for (int i = 0; i < screenCount; i++)
                {
                    Screen screen = Screen.AllScreens[i];
                    if (screen.Primary)
                    {
                        for (int j = 0; j < screenCount; j++)
                        {
                            if (i != j && screen.Bounds == Screen.AllScreens[j].Bounds)
                            {
                                isMirrored = true;
                                break;
                            }
                        }
                        break;
                    }
                }
                if (isMirrored)
                {
                    isExtend = false;
                }
                else
                {
                    isExtend = true;
                }
            }
            return isExtend;
        }

        public static void ChangeDisplayMode(string mode)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "DisplaySwitch.exe",
                Arguments = mode,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
