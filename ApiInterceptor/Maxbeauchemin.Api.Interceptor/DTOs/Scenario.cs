namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class Scenario
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool DisableWarningLog { get; set; }
    public ScenarioFilter Filter { get; set; }
    public ScenarioActions Actions { get; set; }
}