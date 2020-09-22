// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using NLog;

namespace DailyDocuments
{
    /// <summary>
    ///  Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region NLog
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        #endregion NLog

        protected override void OnStartup(StartupEventArgs e)
        {
            string myExeName = Assembly.GetExecutingAssembly().GetName().Name;
            Process currentProcess = Process.GetCurrentProcess();
            Debug.WriteLine($"+++ Current process = {currentProcess.ProcessName} {currentProcess.Id}");

            foreach (var AllProcesses in Process.GetProcesses())
            {
                if (AllProcesses.Id != currentProcess.Id && AllProcesses.ProcessName == currentProcess.ProcessName)
                {
                    string msg = $"* I am  {currentProcess.ProcessName} {currentProcess.Id}. " +
                                 $"- {AllProcesses.ProcessName} {AllProcesses.Id} is also running.";
                    log.Warn(msg);
                    log.Warn($"* An instance of {myExeName} is already running!  Shutting this one down.");

                    _ = MessageBox.Show($"An instance of {myExeName} is already running",
                                        myExeName,
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation,
                                        MessageBoxResult.OK,
                                        MessageBoxOptions.DefaultDesktopOnly);

                    Environment.Exit(1);
                    break;
                }
            }
            base.OnStartup(e);
        }
    }
}