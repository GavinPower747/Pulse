namespace Pulse.WebApp.Configuration;

public class ModuleInfo
{
    public required string Type { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
