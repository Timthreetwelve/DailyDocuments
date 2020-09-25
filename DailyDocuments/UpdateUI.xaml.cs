// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region using directives
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using TKUtils;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
#endregion using directives

namespace DailyDocuments
{
    public partial class UpdateUI : Window
    {
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
            if (EntryClass.Entries.Count > 0)
            {
                listbox1.ItemsSource = EntryClass.Entries;
                listbox1.SelectedItem = EntryClass.Entries.First();
                _ = listbox1.Focus();
            }
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

        #region Check for "untitled" entries in the list box
        private bool CheckForUntitled()
        {
            // Loop through the list backwards checking for null titles
            for (int i = listbox1.Items.Count - 1; i >= 0; i--)
            {
                object item = listbox1.Items[i];
                EntryClass x = item as EntryClass;
                if (string.IsNullOrEmpty(x.Title))
                {
                    log.Error("New item prohibited, \"untitled\" entry in list");
                    _ = MessageBox.Show("Please update or delete the untitled entry before adding another new entry.",
                                    "DailyDocuments List Maintenance Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                    return false;
                }
            }
            return true;
        }
        #endregion Check for "untitled" entries in the list box

        #region New item
        private void NewItem()
        {
            tb1.Text = string.Empty;
            tb2.Text = string.Empty;
            tb3.Text = string.Empty;
            EntryClass newitem = new EntryClass { Title = string.Empty };
            EntryClass.Entries.Add(newitem);
            listbox1.SelectedItem = EntryClass.Entries.Last();
            listbox1.ScrollIntoView(listbox1.SelectedItem);
            _ = tb1.Focus();
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
            for (int i = EntryClass.Entries.Count - 1; i >= 0; i--)
            {
                object item = listbox1.Items[i];
                EntryClass x = item as EntryClass;
                if (string.IsNullOrEmpty(x.Title))
                {
                    _ = EntryClass.Entries.Remove(x);
                    log.Info("Removing null entry");
                }
            }
        }
        #endregion Window Closing

        #region Button events
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (CheckForUntitled())
            {
                NewItem();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            EntryClass.Entries.Remove((EntryClass)listbox1.SelectedItem);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveJson();
            Close();
        }

        private void BtnFilePicker_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Choose a File",
                Multiselect = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };
            if (!string.IsNullOrEmpty(tb2.Text) && File.Exists(tb2.Text.Trim()))
            {
                dlgOpen.FileName = tb2.Text;
            }
            bool? result = dlgOpen.ShowDialog();
            if (result == true)
            {
                tb2.Text = dlgOpen.FileName;
                EntryClass entry = (EntryClass)listbox1.SelectedItem;
                entry.DocumentPath = tb2.Text;
                _ = tb3.Focus();
            }
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

        #region Text in textbox changed event
        private void Tb3_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-zA-Z ,/-]", "");
            textboxSender.SelectionStart = cursorPosition;
        }
        #endregion Text in textbox changed event

        #region Textbox lost focus event
        private void Tb3_LostFocus(object sender, RoutedEventArgs e)
        {
            EntryClass entry = (EntryClass)listbox1.SelectedItem;
            entry.DayCodes = InsertCommas(tb3.Text);
        }
        #endregion Textbox lost focus event

        #region Insert commas in day codes field
        private static string InsertCommas(string str)
        {
            if (str?.Contains(' ') != true)
            {
                return str;
            }
            else
            {
                List<string> chunks = str.Split(new char[] { ' ', ',' })
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s.Trim()))
                    .ToList();
                return string.Join(", ", chunks);
            }
        }
        #endregion Insert commas in day codes field
    }
}
