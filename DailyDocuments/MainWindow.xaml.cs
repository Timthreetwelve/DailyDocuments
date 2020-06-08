// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using TKUtils;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace DailyDocuments
{
    public partial class MainWindow : Window
    {
        private EntryCollection entryCollection;
        private int openDelay;
        private bool matched;
        private DateTime? pickerDate;

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

            DetermineDate(out DateTime date, out int totalDays);

            ReadXml(GetXmlFile());

            GetIcons();

            CheckItems(date, totalDays);

            ProcessCommandLine();

            DataContext = entryCollection.Entries;  // ItemsSource is set in Xaml
        }

        #region Read Settings

        private void ReadSettings()
        {
            WriteLog.WriteTempFile(" ");
            WriteLog.WriteTempFile("DailyDocuments is starting up");
            WindowTitleVersion();
            WriteLog.WriteTempFile("  Settings:");

            Properties.Settings.Default.SettingChanging += SettingChanging;

            // Settings upgrade if needed
            if (Properties.Settings.Default.SettingsUpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsUpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            // Window position
            Top = Properties.Settings.Default.WindowTop;
            Left = Properties.Settings.Default.WindowLeft;
            Height = Properties.Settings.Default.WindowHeight;
            Width = Properties.Settings.Default.WindowWidth;

            // Topmost
            if (Properties.Settings.Default.Topmost)
            {
                Topmost = true;
                mnuOnTop.IsChecked = true;
            }
            else
            {
                Topmost = false;
                mnuOnTop.IsChecked = false;
            }
            WriteLog.WriteTempFile($"    Window Topmost is {Properties.Settings.Default.Topmost}");


            // Open delay
            openDelay = Properties.Settings.Default.OpenDelay;

            switch (openDelay)
            {
                case 250:
                    {
                        mnuDelay25.IsChecked = true;
                        break;
                    }
                case 500:
                    {
                        mnuDelay50.IsChecked = true;
                        break;
                    }
                default:
                    {
                        mnuDelay1.IsChecked = true;
                        break;
                    }
            }
            WriteLog.WriteTempFile($"    Open delay is {openDelay} ms");

            // Exit on open
            if (Properties.Settings.Default.ExitOnOpen)
            {
                mnuExitOpen.IsChecked = true;
            }
            else
            {
                mnuExitOpen.IsChecked = false;
            }
            WriteLog.WriteTempFile($"    Exit on open is {Properties.Settings.Default.ExitOnOpen}");

            // Font size
            lbDocs.FontSize = Properties.Settings.Default.FontSize;
            switch (lbDocs.FontSize)
            {
                case 13:
                    {
                        mnuFontS.IsChecked = true;
                        WriteLog.WriteTempFile($"    Font size is Small");
                        break;
                    }
                case 17:
                    {
                        mnuFontL.IsChecked = true;
                        WriteLog.WriteTempFile($"    Font size is Medium");
                        break;
                    }
                case 15:
                default:
                    {
                        mnuFontM.IsChecked = true;
                        WriteLog.WriteTempFile($"    Font size is Large");
                        break;
                    }
            }

            // Show file type icons
            if (Properties.Settings.Default.ShowFileIcons)
            {
                mnuShowIcons.IsChecked = true;
                Debug.WriteLine("Show file type icons is True");
            }
            else
            {
                mnuShowIcons.IsChecked = false;
                Debug.WriteLine("Show file type icons is False");
            }
        }

        #endregion Read Settings

        #region Fun with Dates

        private void DetermineDate(out DateTime date, out int totalDays)
        {
            if (pickerDate == null)
            {
                pickerDate = DateTime.Now;
                xceedDateTime.Value = pickerDate;
            }

            date = (DateTime)pickerDate;
            DateTime startDate = new DateTime(2019, 1, 1);
            TimeSpan span = (date - startDate);
            totalDays = span.Days;

            WriteLog.WriteTempFile($"  Selected date is: {date.DayOfWeek}, {date}");
            WriteLog.WriteTempFile($"  Total days since 2019/01/01 is: {totalDays}");
            if (totalDays % 2 == 0)
            {
                WriteLog.WriteTempFile("  D2 items will be selected today");
            }
            else
            {
                WriteLog.WriteTempFile("  DA items will be selected today");
            }
        }

        #endregion Fun with Dates

        #region Get the menu XML file name

        private string GetXmlFile()
        {
            // Get our own folder
            string myExePath = Assembly.GetExecutingAssembly().Location;

            // Append the filename
            string myXml = Path.Combine(Path.GetDirectoryName(myExePath), "DailyDocuments.xml");

            return myXml;
        }

        #endregion Get the menu XML file name

        #region Read the XML file

        private void ReadXml(string sourceXML)
        {
            WriteLog.WriteTempFile($"  Reading {sourceXML}");
            XmlSerializer deserializer = new XmlSerializer(typeof(EntryCollection));
            if (File.Exists(sourceXML))
            {
                using (StreamReader reader = new StreamReader(sourceXML))
                {
                    entryCollection = (EntryCollection)deserializer.Deserialize(reader);
                    reader.Close();
                }
            }
            else
            {
                _ = MessageBox.Show($"Error reading XML file\n\nFile not found: {sourceXML}", "DailyDocuments Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(9);
            }
            if (entryCollection.Entries.Length == 1)
            {
                WriteLog.WriteTempFile($"  Read {entryCollection.Entries.Length} entry");
            }
            else
            {
                WriteLog.WriteTempFile($"  Read {entryCollection.Entries.Length} entries");
            }
        }

        #endregion Read the XML file

        #region Get file icons
        private void GetIcons()
        {
            if (Properties.Settings.Default.ShowFileIcons)
            {
                foreach (Entry item in entryCollection.Entries)
                {
                    string docPath = item.DocumentPath.TrimEnd('\\');
                    if (File.Exists(docPath))
                    {
                        Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(docPath);
                        item.FileIcon = IconToImageSource(temp);
                        Debug.WriteLine($"{item.DocumentPath} is a file");
                    }
                    // expand environmental variables for folders
                    else if (Directory.Exists(Environment.ExpandEnvironmentVariables(docPath)))
                    {
                        Icon temp = Properties.Resources.folder_icon;
                        item.FileIcon = IconToImageSource(temp);
                        Debug.WriteLine($"{item.DocumentPath} is a directory");
                    }
                    // if complete path wasn't supplied check the path
                    else if (docPath.EndsWith(".exe"))
                    {
                        StringBuilder sb = new StringBuilder(docPath, 2048);
                        bool found = PathFindOnPath(sb, new String[] { null });
                        if (found)
                        {
                            Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(sb.ToString());
                            item.FileIcon = IconToImageSource(temp);
                        }
                        else
                        {
                            Icon temp = Properties.Resources.question_icon;
                            item.FileIcon = IconToImageSource(temp);
                        }
                        Debug.WriteLine($"{item.DocumentPath} is executable");
                    }
                    else
                    {
                        Icon temp = Properties.Resources.question_icon;
                        item.FileIcon = IconToImageSource(temp);
                        Debug.WriteLine($"{item.DocumentPath} is something else");
                    }
                }
            }
        }

        private ImageSource IconToImageSource(Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
        }

        #endregion Get file icons

        #region Edit the XML file
        private void EditXML()
        {
            string sourceXML = GetXmlFile();
            try
            {
                _ = Process.Start(sourceXML);
            }
            catch (Win32Exception ex)
            {
                // If no application associate with .xml, fire up notepad.exe
                if (ex.NativeErrorCode == 1155)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "notepad.exe";
                    p.StartInfo.Arguments = sourceXML;
                    _ = p.Start();
                }
                else
                {
                    _ = MessageBox.Show($"Error reading XML file {sourceXML}\n{ex.Message}", "DailyDocuments Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading XML file {sourceXML}\n{ex.Message}", "DailyDocuments Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Edit the XML file

        #region Check the boxes based on date
        private void CheckItems(DateTime theDate, int totalDays)
        {
            foreach (Entry item in entryCollection.Entries)
            {
                if (item.Frequency == null) continue;
                matched = false;

                foreach (string d in item.Frequency)
                {
                    if (matched) continue;

                    string day = d.Trim().ToUpper();

                    // D1 = Every day
                    if (day == "D1")
                    {
                        CheckItem(item, day);
                    }
                    // D2 every two days
                    else if (day == "D2" && (totalDays % 2 == 0))
                    {
                        CheckItem(item, day);
                    }
                    // DA every two days (the other two)
                    else if (day == "DA" && (totalDays + 1) % 2 == 0)
                    {
                        CheckItem(item, day);
                    }
                    // SU = Sunday
                    else if (day == "SU" && theDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        CheckItem(item, day);
                    }
                    // MO = Monday
                    else if (day == "MO" && theDate.DayOfWeek == DayOfWeek.Monday)
                    {
                        CheckItem(item, day);
                    }
                    // TU = Tuesday
                    else if (day == "TU" && theDate.DayOfWeek == DayOfWeek.Tuesday)
                    {
                        CheckItem(item, day);
                    }
                    // WE = Wednesday
                    else if (day == "WE" && theDate.DayOfWeek == DayOfWeek.Wednesday)
                    {
                        CheckItem(item, day);
                    }
                    // TH = Thursday
                    else if (day == "TH" && theDate.DayOfWeek == DayOfWeek.Thursday)
                    {
                        CheckItem(item, day);
                    }
                    // FR = Friday
                    else if (day == "FR" && theDate.DayOfWeek == DayOfWeek.Friday)
                    {
                        CheckItem(item, day);
                    }
                    // SA = Saturday
                    else if (day == "SA" && theDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        CheckItem(item, day);
                    }
                   // Monthly
                    else if (day.StartsWith("M"))
                    {
                        string dt = day.Trim().Substring(1);

                        // Last day of the month
                        if (dt.StartsWith("L"))
                        {
                            int lastDayOfMonth = DateTime.DaysInMonth(theDate.Year, theDate.Month);

                            if (theDate.Day == lastDayOfMonth)
                            {
                                CheckItem(item, day);
                            }
                        }
                        else
                        {
                            // Monthly on a specific day
                            bool res = int.TryParse(dt, out int monthDay);
                            if (res && theDate.Day == monthDay)
                            {
                                CheckItem(item, day);
                            }
                            else
                            {
                                WriteLog.WriteTempFile($"  No match for {day}: \"{item.DocumentPath}\"");
                            }
                        }
                    }
                    // Yearly
                    else if (day.StartsWith("Y"))
                    {
                        string dt = day.Trim().Substring(1);

                        bool res = int.TryParse(dt, out int yearDay);
                        if (res && theDate.DayOfYear == yearDay)
                        {
                            CheckItem(item, day);
                        }
                        else
                        {
                            WriteLog.WriteTempFile($"  No match for {day}: \"{item.DocumentPath}\"");
                        }
                    }
                    // Specific date
                    else if (day.Contains("/") || day.Contains("-"))
                    {
                        string trimmed = day.Trim().Replace('-', '/');
                        string mo = trimmed.Substring(0, trimmed.IndexOf("/"));
                        string da = trimmed.Substring(trimmed.IndexOf("/") + 1);
                        bool resa = int.TryParse(mo, out int moNum);
                        bool resb = int.TryParse(da, out int daNum);
                        if (resa && resb && theDate.Month == moNum && theDate.Day == daNum)
                        {
                            CheckItem(item, day);
                        }
                        else
                        {
                            WriteLog.WriteTempFile($"  No match for {day}: \"{item.DocumentPath}\"");
                        }
                    }
                    // Even day of the month
                    else if (day == "EVEN" && (theDate.Day % 2 == 0))
                    {
                        CheckItem(item, day);
                    }
                    // Odd day of the month
                    else if (day == "ODD" && (theDate.Day % 2 == 1))
                    {
                        CheckItem(item, day);
                    }
                    // No match
                    else
                    {
                        WriteLog.WriteTempFile($"  No match for {day}: \"{item.DocumentPath}\"");
                    }
                }
            }
        }

        private void CheckItem(Entry item, string day)
        {
            item.IsChecked = true;
            matched = true;
            WriteLog.WriteTempFile($"  Matched {day}:  \"{item.Title}\"");
        }

        #endregion Check the boxes based on date

        #region Button events

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteTempFile($"  Opening selected items");
            foreach (var item in entryCollection.Entries)
            {
                using (Process launch = new Process())
                {
                    Debug.WriteLine($"{item.Title} is {item.IsChecked}");
                    if (item.IsChecked)
                    {
                        try
                        {
                            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.DocumentPath);
                            _ = launch.Start();
                            WriteLog.WriteTempFile($"  Opening \"{item.Title}\"");
                            Thread.Sleep(openDelay);
                        }
                        catch (Exception ex)
                        {
                            _ = MessageBox.Show($"Error launching \"{item.Title}\"" +
                                                $"\n{item.DocumentPath}" +
                                                $"\n\n{ex.Message}",
                                                "Launch Error",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Error);

                            WriteLog.WriteTempFile($"* Open failed for {item.Title}");
                            WriteLog.WriteTempFile($"* {ex.Message}");
                        }
                    }
                }
            }
            if (Properties.Settings.Default.ExitOnOpen)
            {
                Application.Current.Shutdown();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            RefreshEntries();
        }

        #endregion Button events

        #region Window events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.WindowLeft = Left;
            Properties.Settings.Default.WindowTop = Top;
            Properties.Settings.Default.WindowHeight = Height;
            Properties.Settings.Default.WindowWidth = Width;

            // save the property settings
            Properties.Settings.Default.Save();

            WriteLog.WriteTempFile("DailyDocuments is shutting down");
        }
        #endregion Window events

        #region DatePicker changed events
        private void XceedDateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded)
            {
                pickerDate = xceedDateTime.Value;

                DetermineDate(out DateTime date, out int totalDays);

                RefreshEntries();

                CheckItems(date, totalDays);

                Debug.WriteLine($"Selected date is {pickerDate}");
                WriteLog.WriteTempFile($"  Selected date changed to {pickerDate} - Reloading...");
            }
        }
        #endregion

        #region Menu events

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MnuEditXML_Click(object sender, RoutedEventArgs e)
        {
            EditXML();
        }

        private void MnuReload_Click(object sender, RoutedEventArgs e)
        {
            WriteLog.WriteTempFile($"  Reloading XML file.");
            DetermineDate(out DateTime date, out int totalDays);
            ReadXml(GetXmlFile());
            GetIcons();
            lbDocs.ItemsSource = entryCollection.Entries;
            CheckItems(date, totalDays);
        }

        private void MnuOnTop_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Topmost = true;
        }

        private void MnuOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Topmost = false;
        }

        private void MnuFontS_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FontSize = 13;
            mnuFontM.IsChecked = false;
            mnuFontL.IsChecked = false;
        }

        private void MnuFontM_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FontSize = 15;
            mnuFontS.IsChecked = false;
            mnuFontL.IsChecked = false;
        }

        private void MnuFontL_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FontSize = 17;
            mnuFontS.IsChecked = false;
            mnuFontM.IsChecked = false;
        }

        private void MnuExitOpen_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ExitOnOpen = true;
        }

        private void MnuExitOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ExitOnOpen = false;
        }

        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }

        private void MnuReadme_Click(object sender, RoutedEventArgs e)
        {
            string readme = @".\ReadMe.txt";

            try
            {
                _ = Process.Start(readme);
            }
            catch (Win32Exception ex)
            {
                // If no application is associated with .txt fire up notepad.exe
                if (ex.NativeErrorCode == 1155)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "notepad.exe";
                    p.StartInfo.Arguments = readme;
                    _ = p.Start();
                }
                else
                {
                    _ = MessageBox.Show($"Error reading temp file {readme}\n{ex.Message}", "DailyDocuments Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading temp file {readme}\n{ex.Message}", "DailyDocuments Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuViewTemp_Click(object sender, RoutedEventArgs e)
        {
            string temp = WriteLog.GetTempFile();
            try
            {
                _ = Process.Start(temp);
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1155)
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = "notepad.exe";
                        p.StartInfo.Arguments = temp;
                        _ = p.Start();
                    }
                }
                else
                {
                    _ = MessageBox.Show($"Error reading temp file {temp}\n{ex.Message}", "DailyDocuments Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error reading temp file {temp}\n{ex.Message}", "DailyDocuments Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuDelay25_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OpenDelay = 250;
            mnuDelay50.IsChecked = false;
            mnuDelay1.IsChecked = false;
        }

        private void MnuDelay50_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OpenDelay = 500;
            mnuDelay25.IsChecked = false;
            mnuDelay1.IsChecked = false;
        }

        private void MnuDelay1_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OpenDelay = 1000;
            mnuDelay25.IsChecked = false;
            mnuDelay50.IsChecked = false;
        }

        private void MnuShowIcons_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowFileIcons = true;
        }

        private void MnuShowIcons_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowFileIcons = false;
        }
        private void CxmOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lbDocs.SelectedItem != null)
            {
                Entry entry = (Entry)lbDocs.SelectedItem;
                Debug.WriteLine(entry.DocumentPath);
                using (Process p = new Process())
                {
                    try
                    {
                        p.StartInfo.FileName = Environment.ExpandEnvironmentVariables(entry.DocumentPath);
                        _ = p.Start();
                        WriteLog.WriteTempFile($"  Opening \"{entry.Title}\"");
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Error launching \"{entry.Title}\"" +
                                            $"\n{entry.DocumentPath}" +
                                            $"\n\n{ex.Message}",
                                            "Launch Error",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                    }
                }
            }

        }

        #endregion Menu events

        #region Mouse events
        void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSingleDoc(sender);
        }

        private static void OpenSingleDoc(object sender)
        {
            if (!(((ListViewItem)sender).Content is Entry entry))
            {
                return;
            }

            Debug.WriteLine($"+++ {entry.DocumentPath}");

            using (Process p = new Process())
            {
                try
                {
                    p.StartInfo.FileName = Environment.ExpandEnvironmentVariables(entry.DocumentPath);
                    _ = p.Start();
                    WriteLog.WriteTempFile($"  Opening \"{entry.Title}\"");
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Error launching \"{entry.Title}\"" +
                                        $"\n{entry.DocumentPath}" +
                                        $"\n\n{ex.Message}",
                                        "Launch Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Setting changed events

        private void SettingChanging(object sender, SettingChangingEventArgs e)
        {
            switch (e.SettingName)
            {
                case "FontSize":
                    {
                        Debug.WriteLine($"{e.SettingName} {e.NewValue}");
                        lbDocs.FontSize = (double)e.NewValue;
                        break;
                    }
                case "Topmost":
                    {
                        Debug.WriteLine($"{e.SettingName} {e.NewValue}");
                        Topmost = (bool)e.NewValue;
                        break;
                    }
                case "ExitOnOpen":
                    {
                        Debug.WriteLine($"{e.SettingName} {e.NewValue}");
                        break;
                    }
                case "OpenDelay":
                    {
                        Debug.WriteLine($"{e.SettingName} {e.NewValue}");
                        openDelay = (int)e.NewValue;
                        break;
                    }
                case "ShowFileIcons":
                    {
                        Debug.WriteLine($"{e.SettingName} {e.NewValue}");
                        break;
                    }
            }
        }

        #endregion Setting changed

        #region Keyboard events

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // F1 opens about
            if (e.Key == Key.F1)
            {
                About ab = new About();
                ab.Show();
            }
            // Ctrl + comma opens Preferences
            if (e.Key == Key.OemComma && (e.KeyboardDevice.Modifiers == ModifierKeys.Control))
            {
                mnuFile.IsSubmenuOpen = true;
                mnuPrefs.IsSubmenuOpen = true;
            }
            // Ctrl + E opens the XML file
            if (e.Key == Key.E && (e.KeyboardDevice.Modifiers == ModifierKeys.Control))
            {
                EditXML();
            }
        }

        #endregion Keyboard events

        #region Title version

        public void WindowTitleVersion()
        {
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string myExe = Assembly.GetExecutingAssembly().GetName().Name;

            // Remove the release (last) node
            string titleVer = version.ToString().Remove(version.ToString().LastIndexOf("."));

            // Set the windows title
            Title = string.Format($"{myExe} - {titleVer}");

            WriteLog.WriteTempFile($"  {myExe} version is {titleVer}");
        }

        #endregion Title version

        #region Find on path

        // This will search the Windows Path for the specified file
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);
        #endregion

        #region Command line arguments
        private void ProcessCommandLine()
        {
            foreach (string item in Environment.GetCommandLineArgs())
            {
                // If command line argument "auto" is found click "Open" button
                if (item.Replace("-","").Replace("/","").ToLower().Equals("automatic"))
                {
                    btnOpen.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WriteLog.WriteTempFile($"  Command line argument \"{item}\" found. Opening matched items.");
                }
            }
        }
        #endregion

        #region Refresh list
        private void RefreshEntries()
        {
            foreach (var entry in entryCollection.Entries)
            {
                entry.IsChecked = false;
            }
            lbDocs.Items.Refresh();
        }
        #endregion

    }
}