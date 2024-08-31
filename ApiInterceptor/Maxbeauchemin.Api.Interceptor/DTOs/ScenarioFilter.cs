namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class ScenarioFilter
{
    public List<string>? Identities { get; set; }
    public List<FilterEndpoint>? Endpoints { get; set; }
    public int? Percentage { get; set; }
}