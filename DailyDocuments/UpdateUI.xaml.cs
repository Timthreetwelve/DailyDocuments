// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

#region using directives
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using NLog;
using TKUtils;
#endregion using directives

namespace DailyDocuments
{
    public partial class UpdateUI : Window
    {
        public bool entriesChanged;

        #region NLog
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        #endregion NLog

        public UpdateUI()
        {
            InitializeComponent();

            LoadListBox();

            WhatDay();
        }

        #region Load the list box
        private void LoadListBox()
        {
            listbox1.ItemsSource = EntryClass.Entries;
            listbox1.SelectedItem = EntryClass.Entries.First();
            _ = listbox1.Focus();
        }
        #endregion Load the list box

        #region Determine if it's a D2 or DA day
        private void WhatDay()
        {
            if (MainWindow.totalDays % 2 == 0)
            {
                tbkWhatDay.Text = "D2";
                tbkWhatDay.ToolTip = "Today is a D2 day";
            }
            else
            {
                tbkWhatDay.Text = "DA";
                tbkWhatDay.ToolTip = "Today is a DA day";
            }
        }
        #endregion Determine if it's a D2 or DA day

        #region New item
        private void New_Click(object sender, RoutedEventArgs e)
        {
            tb1.Text = string.Empty;
            tb2.Text = string.Empty;
            tb3.Text = string.Empty;
            EntryClass newitem = new EntryClass { Title = string.Empty };
            EntryClass.Entries.Add(newitem);
            listbox1.SelectedItem = EntryClass.Entries.Last();
            listbox1.ScrollIntoView(listbox1.SelectedItem);
            _ = listbox1.Focus();
        }
        #endregion New item

        #region Save collection
        private void SaveJson()
        {
            ObservableCollection<EntryClass> tempCollection = new ObservableCollection<EntryClass>();

            foreach (EntryClass item in EntryClass.Entries)
            {
                if (!string.IsNullOrEmpty(item.Title) && (!string.IsNullOrEmpty(item.DocumentPath)))
                {
                    EntryClass ec = new EntryClass
                    {
                        Title = item.Title,
                        DocumentPath = item.DocumentPath,
                        DayCodes = item.DayCodes,
                        FileIcon = item.FileIcon,
                        IsChecked = item.IsChecked
                    };
                    tempCollection.Add(ec);
                }
            }

            string json = JsonConvert.SerializeObject(tempCollection, Formatting.Indented);
            try
            {
                log.Info($"Saving JSON file: {GetJsonFile()}");
                File.WriteAllText(GetJsonFile(), json);
            }
            catch (System.Exception ex)
            {
                log.Error(ex, "Error saving file.");
                SystemSounds.Exclamation.Play();
                _ = MessageBox.Show($"Error saving file.\n\n{ex}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
        }
        #endregion Save collection

        #region Get the JSON file name
        private string GetJsonFile()
        {
            return Path.Combine(AppInfo.AppDirectory, "DailyDocuments.json");
        }
        #endregion Get the JSON file name

        #region Window Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }
        #endregion Window Closing

        #region Button events
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            EntryClass.Entries.Remove((EntryClass)listbox1.SelectedItem);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveJson();
            Close();
        }
        #endregion Button events

        #region Menu events
        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveJson();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowAbout();
        }

        private void DayCodesHelp(object sender, RoutedEventArgs e)
        {
            if (IsWindowInstantiated<TextViewer>())
                return;

            string msg = Properties.Resources.DayCodesHelp;
            TextViewer tv = new TextViewer();
            tv.txtViewer.Text = msg;
            tv.Title = "DailyDocuments - Day Codes Help";
            tv.Show();
        }
        #endregion Menu events

        #region Keyboard Events
        // Enter key moves focus to next textbox
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.Source is TextBox s)
                {
                    s.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
                e.Handled = true;
            }
        }
        #endregion

        #region Is day codes help window already open?
        public static bool IsWindowInstantiated<T>() where T : Window
        {
            var windows = Application.Current.Windows.Cast<Window>();
            return windows.Any(s => s is T);
        }
        #endregion Is day codes help window already open?
    }
}
