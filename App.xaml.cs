using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Snow_RunGame
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        string GameFile = "vsac25_Release.exe";
        System.Threading.Mutex mutex;

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, GameFile, out ret);
            if (!ret)
            {
                MessageBox.Show("检测到程序已运行，请勿重复启动", "警告");
                Environment.Exit(0);
            }
            else
            {
                RunGameAsync();
            }

        }
        internal static class User32
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindow(IntPtr hWnd);
        }
        private Process externalProcess;
        private async Task RunGameAsync()
        {
            if (File.Exists(GameFile))   //该方法是判断该路径下有没有该文件，注意加上文件后缀名
            {
                externalProcess = Process.Start(GameFile);

                await Task.Run(() =>
                {
                    while (!externalProcess.HasExited)
                    {
                        IntPtr mainWindowHandle = externalProcess.MainWindowHandle;
                        if (mainWindowHandle != IntPtr.Zero && !User32.IsWindow(mainWindowHandle))
                        {
                            externalProcess.Kill();

                            //// 外部进程窗口关闭后，关闭WPF应用程序
                            //Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    Application.Current.Shutdown();
                            //});

                            break;
                        }

                        // 延时以避免过度占用CPU
                        Thread.Sleep(500);
                    }

                    // 外部进程窗口关闭后，关闭WPF应用程序
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                });
            }
            else
            {
                MessageBox.Show($"未检测到 {GameFile} 文件，请检查游戏路径是否正确！", "错误");
                Environment.Exit(0);
                return;
            }

        }

    }
}
