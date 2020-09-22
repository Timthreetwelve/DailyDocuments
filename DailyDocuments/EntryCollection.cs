﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DailyDocuments
{
    public class Entry : INotifyPropertyChanged
    {
        private string title;
        private string documentPath;
        private string dayCodes;

        [XmlElement("Title")]
        public string Title
        {
            get { return title; }
            set
            {
                if (value != null)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlElement("DocumentPath")]
        public string DocumentPath
        {
            get { return documentPath; }
            set
            {
                if (value != null)
                {
                    documentPath = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlElement("DayCodes")]
        public string DayCodes
        {
            get { return dayCodes; }
            set
            {
                if (value != null)
                {
                    dayCodes = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public bool IsChecked { get; set; }

        [XmlIgnore]
        public ImageSource FileIcon { get; set; }

        // Property changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [XmlRoot("EntryCollection")]
    public class EntryCollection
    {
        [XmlElement("Entry")]
        public Entry[] Entries { get; set; }

        public static ObservableCollection<EntryCollection> EntryColl { get; set; }
    }
}