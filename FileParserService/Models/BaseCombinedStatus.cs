using System.Text.Json.Serialization;

namespace FileParserService.Models;

[JsonDerivedType(typeof(CombinedSamplerStatus), typeDiscriminator: "sampler")]
[JsonDerivedType(typeof(CombinedPumpStatus), typeDiscriminator: "pump")]
[JsonDerivedType(typeof(CombinedOvenStatus), typeDiscriminator: "oven")]
public abstract class BaseCombinedStatus
{
    public required string ModuleState { get; set; }
    public bool IsBusy { get; set; }
    public bool IsReady { get; set; }
    public bool IsError { get; set; }
    public bool KeyLock { get; set; }
}