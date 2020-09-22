// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

// Comment out the following if MessageBox is not to be used
#define messagebox

#region using directives
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using NLog;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
#endregion

namespace TKUtils
{
    /// <summary>
    ///  Class for viewing text files. If the file extension is not associated
    ///  with an application, notepad.exe will be attempted.
    /// </summary>
    public static class TextFileViewer
    {
        #region Text file viewer
        /// <summary>
        /// Opens specified text file
        /// </summary>
        /// <param name="txtfile">Full path for text file</param>
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void ViewTextFile(string txtfile)
        {
            if (File.Exists(txtfile))
            {
                try
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = txtfile;
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.ErrorDialog = false;
                        _ = p.Start();
                    }
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1155)
                    {
                        using (Process p = new Process())
                        {
                            p.StartInfo.FileName = "notepad.exe";
                            p.StartInfo.Arguments = txtfile;
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.ErrorDialog = false;
                            _ = p.Start();
                        }
                    }
                    else
                    {
                        log.Error($"* Unable to open {txtfile}");
                        log.Error($"* {ex.Message}");
                        SystemSounds.Exclamation.Play();
#if messagebox
                        _ = MessageBox.Show($"Error opening file\n{txtfile}\n\n{ex.Message}",
                        "DailyDocuments Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
#endif
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"* Unable to open {txtfile}");
                    log.Error($"* {ex.Message}");
                    SystemSounds.Exclamation.Play();
#if messagebox
                    _ = MessageBox.Show("Unable to start default application used to open\n" +
                    $" {txtfile}",
                    "DailyDocumentsError",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
#endif
                }
            }
            else
            {
                log.Error($"File not found: {txtfile}");
                SystemSounds.Exclamation.Play();
#if messagebox
                _ = MessageBox.Show("File not found:\n" +
                $" {txtfile}",
                "DailyDocumentsError",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
#endif
            }
        }
        #endregion
    }
}