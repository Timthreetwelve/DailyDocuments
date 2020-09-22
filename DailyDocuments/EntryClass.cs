// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

#region using directives
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Newtonsoft.Json;
#endregion using directives

namespace DailyDocuments
{
    public class EntryClass : INotifyPropertyChanged
    {
        #region Private backing fields
        private string title;
        private string documentPath;
        private string dayCodes;
        #endregion Private backing fields

        #region Properties
        [JsonProperty("Title")]
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

        [JsonProperty("DocumentPath")]
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

        [JsonProperty("DayCodes")]
        public string DayCodes
        {
            get { return dayCodes; }
            set
            {
                if (value != null)
                {
                    dayCodes = value.ToUpper();
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool IsChecked { get; set; }

        [JsonIgnore]
        public ImageSource FileIcon { get; set; }
        #endregion Properties

        #region Handle property change
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change

        #region Observable collection
        public static ObservableCollection<EntryClass> Entries { get; set; }
        #endregion Observable collection
    }
}
