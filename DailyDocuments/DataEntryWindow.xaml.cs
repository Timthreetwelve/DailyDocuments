using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using TKUtils;

namespace DailyDocuments
{
    /// <summary>
    /// Interaction logic for DataEntryWindow.xaml
    /// </summary>
    public partial class DataEntryWindow : Window
    {
        private EntryCollection entryCollection;
        private int currentRecord;

        public DataEntryWindow()
        {
            InitializeComponent();

            ReadXml(GetXmlFile());

            UpdateTextBoxes(entryCollection.Entries[0]);
            tbxRecNum.Text = (currentRecord + 1).ToString();

            wb1.Navigate(new Uri(GetXmlFile()));
        }

        private void UpdateTextBoxes(Entry entry)
        {
            tbxTitle.Text = entry.Title;
            tbxDocPath.Text = entry.DocumentPath;
            tbxDays.Text = string.Join(", ", entry.DayCodes);
        }

        private Entry NextRecord()
        {
            if (currentRecord < entryCollection.Entries.Length - 1)
            {
                currentRecord++;
                tbxRecNum.Text = (currentRecord + 1).ToString();
                Debug.WriteLine($"current record is now {currentRecord}");
                btnPrev.IsEnabled = true;
                return entryCollection.Entries[currentRecord];
            }
            else
            {
                tbxRecNum.Text = (currentRecord + 1).ToString();
                Debug.WriteLine($"current record is now {currentRecord}");
                btnNext.IsEnabled = false;
                return entryCollection.Entries[currentRecord];
            }
        }
        private Entry PrevRecord()
        {
            if (currentRecord > 0)
            {
                currentRecord--;
                tbxRecNum.Text = (currentRecord + 1).ToString();
                Debug.WriteLine($"current record is now {currentRecord}");
                btnNext.IsEnabled = true;
                return entryCollection.Entries[currentRecord];
            }
            else
            {
                tbxRecNum.Text = (currentRecord + 1).ToString();
                Debug.WriteLine($"current record is now {currentRecord}");
                btnPrev.IsEnabled = false;
                return entryCollection.Entries[0];
            }
        }

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

        #region Get the menu XML file name
        private string GetXmlFile()
        {
            return Path.Combine(AppInfo.AppDirectory, "DailyDocuments.xml");
        }
        #endregion Get the menu XML file name

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxes(PrevRecord());
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxes(NextRecord());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
