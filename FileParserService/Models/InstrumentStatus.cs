using System.Xml.Serialization;

namespace FileParserService.Models;

[XmlRoot("InstrumentStatus")]
public class InstrumentStatus
{
    [XmlAttribute("schemaVersion")]
    public required string SchemaVersion { get; set; }
    public required string PackageID { get; set; }
    
    [XmlElement("DeviceStatus")]
    public List<DeviceStatus> DeviceStatuses { get; set; } = [];
}