namespace Maxbeauchemin.Api.Interceptor.DTOs;

/// <summary>
/// Options provided to control the different interception scenarios
/// </summary>
public class Options
{
    /// <summary>
    /// whether the API interceptor should be enabled
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// the list of scenarios that should be handled. The order listed is the order they will be checked in. The first matching scenario for a request is the only one that will be used (if any).
    /// </summary>
    public List<Scenario> Scenarios { get; set; }
}