using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iomonitor
{
    internal class RunAtStartupSetter
    {
        private const string TaskName = "IomonitorTStartup";
        private const string TaskDescription = "Start Iomonitor at log on";

        public static void RunAtStartupEnable()
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = TaskDescription;

                LogonTrigger logonTrigger = new LogonTrigger();
                td.Triggers.Add(logonTrigger);

                //BootTrigger bootTrigger = new BootTrigger();
                //td.Triggers.Add(bootTrigger);

                //string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                // 使用 GetCurrentProcess 获取当前运行的 .exe 文件路径
                string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                td.Actions.Add(new ExecAction(executablePath, null, null));

                //td.Principal.UserId = "SYSTEM";
                //td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest;

                // 设置任务的启动条件，取消勾选电源设置
                td.Settings.DisallowStartIfOnBatteries = false; // 取消勾选“只有在计算机使用交流电源时才启动此任务”
                td.Settings.StopIfGoingOnBatteries = false; // 取消勾选“如果计算机改用电池电源则停止”
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero; // 取消勾选“如果任务运行时间超过以下时间停止任务”

                ts.RootFolder.RegisterTaskDefinition(TaskName, td);
            }
        }

        public static bool IsRunAtStartup()
        {
            using (TaskService ts = new TaskService())
            {
                return ts.GetTask(TaskName) != null;
            }
        }

        public static void RunAtStartupDisable()
        {
            using (TaskService ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(TaskName, false);
            }
        }
    }
}
