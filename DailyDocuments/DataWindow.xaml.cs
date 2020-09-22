using System.Data;
using System.IO;
using System.Windows;
using TKUtils;

namespace DailyDocuments
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        public DataWindow()
        {
            InitializeComponent();

            DataSet xmlData = new DataSet();
            _ = xmlData.ReadXml(GetXmlFile());

            dg1.ItemsSource = xmlData.Tables[0].DefaultView;
        }

        #region Get the menu XML file name
        private string GetXmlFile()
        {
            return Path.Combine(AppInfo.AppDirectory, "DailyDocuments.xml");
        }
        #endregion Get the menu XML file name
    }
}
