// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Navigation;

namespace DailyDocuments
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            string version = versionInfo.FileVersion;
            string copyright = versionInfo.LegalCopyright;
            string product = versionInfo.ProductName;

            tbVersion.Text = version.Remove(version.LastIndexOf("."));
            tbCopyright.Text = copyright.Replace("Copyright ", "");
            Title = $"About {product}";
            Topmost = true;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            Close();
            _ = Process.Start(".\\ReadMe.txt");
        }
    }
}
