﻿using System;
using System.Threading.Tasks;
using System.Windows;

using NSPersonalCloud.WindowsContract;

namespace NSPersonalCloud.WindowsConfigurator.IPC
{
    public class NotificationCenter : ICloudEventHandler
    {
        public void OnServiceStarted()
        {
            Globals.IsServiceRunning = true;

            Application.Current.Dispatcher.Invoke(() => {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = null;
                }
            });

            Task.Run(async () => {
                var cloud = await Globals.Storage.InvokeAsync(x => x.GetAllPersonalCloud()).ConfigureAwait(false);
                Globals.PersonalCloud = cloud.Length == 0 ? null : (Guid?) cloud[0];

                Application.Current.Dispatcher.Invoke(() => {
                    if (Globals.PersonalCloud != null) Application.Current.MainWindow = new MainWindow();
                    else Application.Current.MainWindow = new WelcomeWindow();
                    Application.Current.MainWindow.Show();
                });
            });
        }

        public void OnLeftPersonalCloud()
        {
            Application.Current.Dispatcher.Invoke(() => {
                var shouldShow = false;
                if (Application.Current.MainWindow != null)
                {
                    if (Application.Current.MainWindow.IsVisible) shouldShow = true;
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = null;
                }

                Globals.PersonalCloud = null;
                Application.Current.MainWindow = new WelcomeWindow();
                if (shouldShow) Application.Current.MainWindow.Show();
            });
        }

        public void OnMountedVolumesChanged()
        {
            // Todo
        }

        public void OnPersonalCloudAdded()
        {
            var shouldShow = false;
            Application.Current.Dispatcher.Invoke(() => {
                if (Application.Current.MainWindow != null)
                {
                    if (Application.Current.MainWindow.IsVisible) shouldShow = true;
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = null;
                }
            });

            Task.Run(async ()=> { 
                var cloud = await Globals.Storage.InvokeAsync(x => x.GetAllPersonalCloud()).ConfigureAwait(false);
                Globals.PersonalCloud = cloud[0];

                Application.Current.Dispatcher.Invoke(() => {
                    Application.Current.MainWindow = new MainWindow();
                    if (shouldShow) Application.Current.MainWindow.Show();
                });
            });
        }

        public void OnServiceStopped()
        {
            Globals.IsServiceRunning = false;

            Application.Current.Dispatcher.Invoke(() => {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = null;
                }
            });
        }

        public void OnVolumeIOError(string mountPoint, Exception exception)
        {
            if (exception is DllNotFoundException)
            {
                Application.Current.ShowAlert("无法加载网络驱动器",
                    "个人云部分组件已损坏，请重新运行个人云安装程序。" + Environment.NewLine + Environment.NewLine + "如果您选择跳过安装网络驱动器组件，您无法使用网络驱动器功能。");
            }
            else
            {
                Application.Current.ShowAlert("网络驱动器非正常断开",
                    $"通过个人云加载的网络驱动器 {mountPoint[0]} 工作异常、即将断开。" + Environment.NewLine + Environment.NewLine + "请重新加载网络驱动器，或联系技术支持。");
            }
        }
    }
}
