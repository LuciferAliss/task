using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SharedKernel;

public class DeviceStatus
{
    public required string ModuleCategoryID { get; set; }
    public int IndexWithinRole { get; set; }

    [JsonIgnore]
    public string? RapidControlStatus { get; set; }

    [XmlIgnore]
    public BaseCombinedStatus? ParsedStatus { get; set; }
}