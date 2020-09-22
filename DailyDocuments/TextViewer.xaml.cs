// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DailyDocuments
{
    public partial class TextViewer : Window
    {
        private double curZoom = 1;

        public TextViewer()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        // Close window when ESC is pressed
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        // Control + Mouse wheel to change text size!
        private void DockPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                if (curZoom < 2)
                {
                    curZoom += .05;
                    txtViewer.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                }
            }
            else if (e.Delta < 0)
            {
                if (curZoom > .7)
                {
                    curZoom -= .05;
                    txtViewer.LayoutTransform = new ScaleTransform(curZoom, curZoom);
                }
            }
        }
    }
}
