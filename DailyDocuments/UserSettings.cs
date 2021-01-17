// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKUtils;

namespace DailyDocuments
{
    public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
    {
        #region Constructor
        public UserSettings()
        {
            // Set defaults
            BGColor = "#FFF5F5F5";
            ExitOnOpen = false;
            FontSize = 15;
            OpenDelay = 500;
            ShowFileIcons = true;
            Topmost = false;
            WindowHeight = 300;
            WindowLeft = 100;
            WindowTop = 100;
            WindowWidth = 300;
        }
        #endregion Constructor

        #region Properties
        public string BGColor
        {
            get => bgColor;
            set
            {
                bgColor = value;
                OnPropertyChanged();
            }
        }

        public bool ExitOnOpen
        {
            get => exitOnOpen;
            set
            {
                exitOnOpen = value;
                OnPropertyChanged();
            }
        }

        public double FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                OnPropertyChanged();
            }
        }

        public int OpenDelay
        {
            get => openDelay;
            set
            {
                openDelay = value;
                OnPropertyChanged();
            }
        }

        public bool ShowFileIcons
        {
            get => showFileIcons;
            set
            {
                showFileIcons = value;
                OnPropertyChanged();
            }
        }

        public bool Topmost
        {
            get => topmost;
            set
            {
                topmost = value;
                OnPropertyChanged();
            }
        }

        public double WindowHeight
        {
            get
            {
                if (windowHeight < 100)
                {
                    windowHeight = 100;
                }
                return windowHeight;
            }
            set => windowHeight = value;
        }

        public double WindowLeft
        {
            get
            {
                if (windowLeft < 0)
                {
                    windowLeft = 0;
                }
                return windowLeft;
            }
            set => windowLeft = value;
        }

        public double WindowTop
        {
            get
            {
                if (windowTop < 0)
                {
                    windowTop = 0;
                }
                return windowTop;
            }
            set => windowTop = value;
        }

        public double WindowWidth
        {
            get
            {
                if (windowWidth < 100)
                {
                    windowWidth = 100;
                }
                return windowWidth;
            }
            set => windowWidth = value;
        }
        #endregion Properties

        #region Private backing fields
        private string bgColor;
        private bool exitOnOpen;
        private double fontSize;
        private int openDelay;
        private bool showFileIcons;
        private bool topmost;
        private double windowHeight;
        private double windowLeft;
        private double windowTop;
        private double windowWidth;
        #endregion Private backing fields

        #region Handle property change event
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change event
    }
}
