using System.Xml.Serialization;

namespace FileParserService.Models
{
    [XmlRoot("InstrumentStatus")]
    public class InstrumentStatus
    {
        [XmlElement("PackageID")]
        public string? PackageID { get; set; }

        [XmlElement("DeviceStatus")]
        public List<DeviceStatus> DeviceStatuses { get; set; } = new List<DeviceStatus>();
    }




}