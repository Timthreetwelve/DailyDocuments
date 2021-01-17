// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;
using TKUtils;
using ColorDialog = System.Windows.Forms.ColorDialog;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
#endregion Using directives

namespace DailyDocuments
{
    public partial class MainWindow : Window
    {
        #region Private fields
        private int openDelay;
        private bool matched;
        private DateTime? pickerDate;
        private DateTime date;
        public static int totalDays;
        #endregion Private fields

        #region NLog
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        #endregion NLog

        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

            InitializeComponent();

            ReadSettings();

            DetermineDate(out date, out totalDays);

            ReadJson(GetJsonFile());

            ConditionalyGetIcons();

            CheckItems(date, totalDays);

            ProcessCommandLine();

            DataContext = EntryClass.Entries;
        }

        #region Read Settings
        private void ReadSettings()
        {
            // Unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Change the log file filename when debugging
            if (Debugger.IsAttached)
                GlobalDiagnosticsContext.Set("TempOrDebug", "debug");
            else
                GlobalDiagnosticsContext.Set("TempOrDebug", "temp");

            log.Info($"{AppInfo.AppName} {AppInfo.TitleVersion} is starting up");

            // Handle changes to settings
            UserSettings.Setting.PropertyChanged += UserSettingChanged;

            // Window position
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;
            Height = UserSettings.Setting.WindowHeight;
            Width = UserSettings.Setting.WindowWidth;

            // Topmost
            Topmost = UserSettings.Setting.Topmost;
            log.Debug($"Window Topmost is {UserSettings.Setting.Topmost}");

            // Open delay
            openDelay = UserSettings.Setting.OpenDelay;
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
            log.Debug($"Open delay is {openDelay} ms");

            // Exit on open
            mnuExitOpen.IsChecked = UserSettings.Setting.ExitOnOpen;
            log.Debug($"Exit on open is {UserSettings.Setting.ExitOnOpen}");

            // Font size
            lbDocs.FontSize = UserSettings.Setting.FontSize;
            switch (lbDocs.FontSize)
            {
                case 13:
                    {
                        mnuFontS.IsChecked = true;
                        log.Debug("Font size is Small");
                        break;
                    }
                case 17:
                    {
                        mnuFontL.IsChecked = true;
                        log.Debug("Font size is Medium");
                        break;
                    }
                default:
                    {
                        mnuFontM.IsChecked = true;
                        log.Debug("Font size is Large");
                        break;
                    }
            }

            // Show file type icons
            mnuShowIcons.IsChecked = UserSettings.Setting.ShowFileIcons;
            log.Debug($"Show file type icons {UserSettings.Setting.ExitOnOpen}");

            // Background color
            string bg = UserSettings.Setting.BGColor;
            lbDocs.Background = BrushFromHex(bg);

            // App name and version in the title
            WindowTitleVersion();
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

            log.Info($"Selected date is: {date.ToLongDateString()}");
            log.Info($"Total days since 2019/01/01 is: {totalDays}");
            if (totalDays % 2 == 0)
            {
                log.Info("D2 items will be selected today");
            }
            else
            {
                log.Info("DA items will be selected today");
            }
        }
        #endregion Fun with Dates

        #region Get the menu JSON file name
        private string GetJsonFile()
        {
            return Path.Combine(AppInfo.AppDirectory, "DailyDocuments.json");
        }
        #endregion Get the menu JSON file name

        #region Read the JSON file
        private void ReadJson(string jsonfile)
        {
            log.Debug($"Reading JSON file: {jsonfile}");
            string json = string.Empty;
            try
            {
                json = File.ReadAllText(jsonfile);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                log.Error(ex, "File or Directory not found {0}", jsonfile);
                SystemSounds.Exclamation.Play();
                _ = MessageBox.Show($"File or Directory not found:\n\n{ex.Message}\n\nUnable to continue.",
                                     "DailyDocuments Error",
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Error);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error reading file: {0}", jsonfile);
                SystemSounds.Exclamation.Play();
                _ = MessageBox.Show($"Error reading file:\n\n{ex.Message}",
                                     "DailyDocuments Error",
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Error);
            }

            EntryClass.Entries = JsonConvert.DeserializeObject<ObservableCollection<EntryClass>>(json);

            if (EntryClass.Entries == null)
            {
                log.Error("File {0} is empty or is invalid", jsonfile);
                SystemSounds.Exclamation.Play();
                _ = MessageBox.Show($"File {jsonfile} is empty or is invalid\n\nUnable to continue.",
                                     "DailyDocuments Error",
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Error);
                Environment.Exit(2);
            }

            if (EntryClass.Entries.Count == 1)
            {
                log.Info($"Read {EntryClass.Entries.Count} entry from {jsonfile}");
            }
            else
            {
                log.Info($"Read {EntryClass.Entries.Count} entries from {jsonfile}");
            }
        }
        #endregion Read the JSON file

        #region Get file icons if needed
        private void ConditionalyGetIcons()
        {
            if (UserSettings.Setting.ShowFileIcons)
            {
                GetIcons();
            }
        }
        #endregion Get file icons if needed

        #region Get file icons
        private void GetIcons()
        {
            foreach (EntryClass item in EntryClass.Entries)
            {
                if (!string.IsNullOrEmpty(item.DocumentPath))
                {
                    string docPath = item.DocumentPath.TrimEnd('\\');
                    if (File.Exists(docPath))
                    {
                        Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(docPath);
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.DocumentPath} is a file");
                    }
                    // expand environmental variables for folders
                    else if (Directory.Exists(Environment.ExpandEnvironmentVariables(docPath)))
                    {
                        Icon temp = Properties.Resources.folder_icon;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.DocumentPath} is a directory");
                    }
                    // if complete path wasn't supplied check the path
                    else if (docPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder(docPath, 2048);
                        bool found = PathFindOnPath(sb, new string[] { null });
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
                        log.Debug($"{item.DocumentPath} is executable");
                    }
                    else if (IsValidURL(docPath))
                    {
                        Icon temp = Properties.Resources.globe_icon;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.DocumentPath} is valid URL");
                    }
                    else
                    {
                        Icon temp = Properties.Resources.question_icon;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.DocumentPath} is something else");
                    }
                }
                else
                {
                    Icon temp = Properties.Resources.question_icon;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug("Document path is empty or null");
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

        #region Edit the JSON file
        private void EditJSON(string json)
        {
            TextFileViewer.ViewTextFile(json);
        }
        #endregion Edit the JSON file

        #region Check the boxes based on date
        private void CheckItems(DateTime theDate, int totalDays)
        {
            foreach (EntryClass item in EntryClass.Entries)
            {
                if (string.IsNullOrEmpty(item.DayCodes))
                    continue;
                matched = false;

                foreach (var d in item.DayCodes.Split(','))
                {
                    if (matched)
                        continue;

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
                                LogNoMatch(item, day);
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
                            LogNoMatch(item, day);
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
                            LogNoMatch(item, day);
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
                    // Weekday
                    else if (day == "WKD" && (theDate.DayOfWeek != DayOfWeek.Sunday && theDate.DayOfWeek != DayOfWeek.Saturday))
                    {
                        CheckItem(item, day);
                    }
                    // Weekend
                    else if (day == "WKE" && (theDate.DayOfWeek == DayOfWeek.Sunday || theDate.DayOfWeek == DayOfWeek.Saturday))
                    {
                        CheckItem(item, day);
                    }
                    // No match
                    else
                    {
                        LogNoMatch(item, day);
                    }
                }
            }
        }

        private static void LogNoMatch(EntryClass item, string day)
        {
            day = day.PadRight(5, ' ');
            log.Debug($"No match:  {day} \"{item.Title}\"");
        }

        private void CheckItem(EntryClass item, string day)
        {
            item.IsChecked = true;
            matched = true;
            day = day.PadRight(5, ' ');
            log.Info($"Matched:   {day} \"{item.Title}\"");
        }
        #endregion Check the boxes based on date

        #region Button events
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Opening selected items");
            foreach (EntryClass item in EntryClass.Entries)
            {
                if (item.IsChecked)
                {
                    LaunchDocument(item);
                }
            }
            if (UserSettings.Setting.ExitOnOpen)
            {
                Application.Current.Shutdown();
            }
        }

        private void LaunchDocument(EntryClass item)
        {
            using (Process launch = new Process())
            {
                try
                {
                    launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.DocumentPath);
                    _ = launch.Start();
                    log.Info($"Opening \"{item.Title}\"");
                    Thread.Sleep(openDelay);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.DocumentPath);
                    SystemSounds.Exclamation.Play();
                    _ = MessageBox.Show($"Error launching \"{item.Title}\"" +
                                        $"\n{item.DocumentPath}" +
                                        $"\n\n{ex.Message}",
                                        "Launch Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            UncheckAll();
        }
        #endregion Button events

        #region Window events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.Setting.WindowLeft = Left;
            UserSettings.Setting.WindowTop = Top;
            UserSettings.Setting.WindowHeight = Height;
            UserSettings.Setting.WindowWidth = Width;

            // save the settings
            UserSettings.SaveSettings();

            log.Info("DailyDocuments is shutting down");

            LogManager.Flush();
        }
        #endregion Window events

        #region DatePicker changed events
        private void XceedDateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded)
            {
                pickerDate = xceedDateTime.Value;
                DateTime pickerD = (DateTime)pickerDate;
                log.Info($"Date changed to { pickerD.ToLongDateString()} - Reloading the list");

                DetermineDate(out date, out totalDays);
                UncheckAll();
                CheckItems(date, totalDays);
            }
        }
        #endregion DatePicker changed events

        #region Menu events
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MnuEditJSON_Click(object sender, RoutedEventArgs e)
        {
            EditJSON(GetJsonFile());
        }

        private void MnuFontS_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.FontSize = 13;
            mnuFontM.IsChecked = false;
            mnuFontL.IsChecked = false;
        }

        private void MnuFontM_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.FontSize = 15;
            mnuFontS.IsChecked = false;
            mnuFontL.IsChecked = false;
        }

        private void MnuFontL_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.FontSize = 17;
            mnuFontS.IsChecked = false;
            mnuFontM.IsChecked = false;
        }

        private void MnuExitOpen_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.ExitOnOpen = true;
        }

        private void MnuExitOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.ExitOnOpen = false;
        }

        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowAbout();
        }

        private void MnuReadme_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(Path.Combine(AppInfo.AppDirectory, "ReadMe.txt"));
        }

        private void MnuViewTemp_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(GetTempFile());
        }

        private void MnuDelay25_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.OpenDelay = 250;
            mnuDelay50.IsChecked = false;
            mnuDelay1.IsChecked = false;
        }

        private void MnuDelay50_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.OpenDelay = 500;
            mnuDelay25.IsChecked = false;
            mnuDelay1.IsChecked = false;
        }

        private void MnuDelay1_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.Setting.OpenDelay = 1000;
            mnuDelay25.IsChecked = false;
            mnuDelay50.IsChecked = false;
        }

        private void MnuMaint_Click(object sender, RoutedEventArgs e)
        {
            OpenMaintWindow();
        }

        private void MnuBGColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog cd = new ColorDialog
            {
                AnyColor = true,
                FullOpen = false,
                Color = BackgroundToColor((SolidColorBrush)lbDocs.Background),
                CustomColors = new int[] { 13882323, 15263718, 15132390, 16119285,
                                          15794175, 14745599, 14417919, 13826810,
                                          16246753, 16708839, 16777184, 16775408,
                                          16775416, 16449525, 16118015, 14810602}
            };
            var result = cd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                lbDocs.Background = BrushFromHex(HexFromColor(cd.Color));
                UserSettings.Setting.BGColor = HexFromColor(cd.Color);
            }
        }

        private void Copy2Clipboard_Click(object sender, RoutedEventArgs e)
        {
            string fullName = AppInfo.AppPath;
            Clipboard.SetText(fullName);
            log.Debug($"{fullName} copied to clipboard");
        }
        #endregion Menu events

        #region Mouse events
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(((ListViewItem)sender).Content is EntryClass))
            {
                return;
            }
            if (lbDocs.SelectedItem != null)
            {
                EntryClass entry = (EntryClass)lbDocs.SelectedItem;
                LaunchDocument(entry);
            }
        }

        private void LbDocs_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            if (e.Delta > 0 && lbDocs.FontSize < 17)
            {
                lbDocs.FontSize += 2;
            }
            else if (e.Delta < 0 && lbDocs.FontSize > 13)
            {
                lbDocs.FontSize -= 2;
            }
            else
            {
                return;
            }

            mnuFontS.IsChecked = false;
            mnuFontM.IsChecked = false;
            mnuFontL.IsChecked = false;

            switch (lbDocs.FontSize)
            {
                case 13:
                    mnuFontS.IsChecked = true;
                    break;
                case 15:
                    mnuFontM.IsChecked = true;
                    break;
                case 17:
                    mnuFontL.IsChecked = true;
                    break;
            }
        }
        #endregion Mouse events

        #region Setting change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            var newValue = prop?.GetValue(sender, null);
            switch (e.PropertyName)
            {
                case "FontSize":
                    lbDocs.FontSize = (double)newValue;
                    break;

                case "Topmost":
                    Topmost = (bool)newValue;
                    break;

                case "OpenDelay":
                    openDelay = (int)newValue;
                    break;

                case "ShowFileIcons":
                    if (IsLoaded && (bool)newValue)
                    {
                        GetIcons();
                        lbDocs.Items.Refresh();
                    }
                    break;
            }
            log.Debug($"***Setting change: {e.PropertyName} New Value: {newValue}");
        }
        #endregion Setting change

        #region Keyboard events
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // F1 opens about
            if (e.Key == Key.F1)
            {
                ShowAbout();
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
                EditJSON(GetJsonFile());
            }
            // Ctrl + M opens the List Maintenance window
            if (e.Key == Key.M && (e.KeyboardDevice.Modifiers == ModifierKeys.Control))
            {
                OpenMaintWindow();
            }
        }
        #endregion Keyboard events

        #region Title version
        public void WindowTitleVersion()
        {
            Title = string.Format($"{AppInfo.AppName} - {AppInfo.TitleVersion}");
        }
        #endregion Title version

        #region Find on path
        /// <summary>
        /// This will search the Windows Path for the specified file
        /// </summary>
        /// <param name="pszFile"></param>
        /// <param name="ppszOtherDirs"></param>
        /// <returns></returns>
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);
        #endregion Find on path

        #region Command line arguments
        private void ProcessCommandLine()
        {
            foreach (string item in Environment.GetCommandLineArgs())
            {
                // If command line argument "auto" is found click "Open" button
                if (item.Replace("-", "").Replace("/", "").Equals("automatic", StringComparison.OrdinalIgnoreCase))
                {
                    btnOpen.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    log.Info($"Command line argument \"{item}\" found. Opening matched items.");
                }
            }
        }
        #endregion Command line arguments

        #region Uncheck all check boxes
        private void UncheckAll()
        {
            foreach (var entry in EntryClass.Entries)
            {
                entry.IsChecked = false;
            }
            lbDocs.Items.Refresh();
        }
        #endregion Uncheck all check boxes

        #region Check for a valid URL
        private static bool IsValidURL(string uriName)
        {
            const string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(uriName);
        }
        #endregion Check for a valid URL

        #region Open the List Maintenance window
        private void OpenMaintWindow()
        {
            UpdateUI maintWindow = new UpdateUI()
            {
                Owner = Application.Current.MainWindow
            };
            maintWindow.ShowDialog();

            log.Info("List Maintenance complete");
            log.Info("Refreshing the main window");

            UncheckAll();

            CheckItems(date, totalDays);

            GetIcons();

            lbDocs.Items.Refresh();
        }
        #endregion Open the List Maintenance window

        #region Show day codes help message box
        private void MnuDayCodesHelp_Click(object sender, RoutedEventArgs e)
        {
            if (IsWindowInstantiated<TextViewer>())
                return;

            string msg = Properties.Resources.DayCodesHelp;
            TextViewer tv = new TextViewer();
            tv.txtViewer.Text = msg;
            tv.Title = "DailyDocuments - Day Codes Help";
            tv.Show();
        }
        #endregion Show day codes help message box

        #region Show the About dialog
        public static void ShowAbout()
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }
        #endregion Show the About dialog

        #region Check to see if windows is already open
        public static bool IsWindowInstantiated<T>() where T : Window
        {
            var windows = Application.Current.Windows.Cast<Window>();
            return windows.Any(s => s is T);
        }
        #endregion Check to see if windows is already open

        #region Get temp file name
        public static string GetTempFile()
        {
            // Ask NLog what the file name is
            var target = LogManager.Configuration.FindTargetByName("logFile") as FileTarget;
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            return target.FileName.Render(logEventInfo);
        }
        #endregion Get temp file name

        #region Color conversions
        private SolidColorBrush BrushFromHex(string hexColorString)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hexColorString);
        }

        private string HexFromColor(System.Drawing.Color color)
        {
            return string.Format("#{0}{1}{2}{3}",
                                 color.A.ToString("X2"),
                                 color.R.ToString("X2"),
                                 color.G.ToString("X2"),
                                 color.B.ToString("X2"));
        }

        private System.Drawing.Color BackgroundToColor(SolidColorBrush brush)
        {
            return System.Drawing.Color.FromArgb(brush.Color.A,
                                  brush.Color.R,
                                  brush.Color.G,
                                  brush.Color.B);
        }
        #endregion Color conversions

        #region Unhandled Exception Handler
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            log.Error("Unhandled Exception");
            Exception e = (Exception)args.ExceptionObject;
            log.Error(e.Message);
            if (e.InnerException != null)
            {
                log.Error(e.InnerException.ToString());
            }
            log.Error(e.StackTrace);

            System.Windows.MessageBox.Show("An Unhandled Exception has occurred.\n" +
                                          $"See {GetTempFile()} for more information.",
                                          "DailyDocuments Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
        }
        #endregion Unhandled Exception Handler
    }
}