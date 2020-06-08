using System.Windows.Media;
using System.Xml.Serialization;

namespace DailyDocuments
{

    public class Entry
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("DocumentPath")]
        public string DocumentPath { get; set; }

        [XmlArray("Frequency")]
        [XmlArrayItem("Day")]
        public string[] Frequency { get; set; }

        [XmlIgnore]
        public bool IsChecked { get; set; }

        [XmlIgnore]
        public ImageSource FileIcon { get; set; }
    }


    [XmlRoot("EntryCollection")]
    public class EntryCollection
    {
        [XmlElement("Entry")]
        public Entry[] Entries { get; set; }
    }
}