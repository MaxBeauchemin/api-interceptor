namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class ScenarioFilter
{
    /// <summary>
    /// if provided, only identities (obtained from the identity provider) that are in the list will be intercepted
    /// </summary>
    public List<string>? Identities { get; set; }
    
    /// <summary>
    /// if provided, only APIs that match one of the items in this collection will be intercepted
    /// </summary>
    public List<FilterEndpoint>? Endpoints { get; set; }
    
    /// <summary>
    /// if provided, a random integer between 1 and 100 will be chosen, and the request will only be intercepted if the number is lower than this parameter's value
    /// </summary>
    public int? Percentage { get; set; }
}