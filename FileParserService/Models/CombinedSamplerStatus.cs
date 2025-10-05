using System.Xml.Serialization;

namespace FileParserService.Models;

[XmlRoot("CombinedSamplerStatus")]
public class CombinedSamplerStatus : BaseCombinedStatus
{
    public int Status { get; set; }
    public required string Vial { get; set; }
    public int Volume { get; set; }
    public int MaximumInjectionVolume { get; set; }
    public required string RackL { get; set; }
    public required string RackR { get; set; }
    public int RackInf { get; set; }
    public bool Buzzer { get; set; }
}