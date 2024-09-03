namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class Scenario
{
    /// <summary>
    /// name of the scenario, will be attached as an `X-Api-Interceptor-Scenario` response header and used for Warning Logs
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// whether this individual scenario should be checked
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// whether Warning logs should be disabled
    /// </summary>
    public bool DisableWarningLog { get; set; }
    
    /// <summary>
    /// the criteria that will be used to identify if a particular request should be intercepted by this scenario
    /// </summary>
    public ScenarioFilter Filter { get; set; }
    
    /// <summary>
    /// the action(s) that will be performed if this scenario intercepts a request
    /// </summary>
    public ScenarioActions Actions { get; set; }
}