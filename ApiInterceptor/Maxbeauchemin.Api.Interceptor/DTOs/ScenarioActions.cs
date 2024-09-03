namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class ScenarioActions
{
    /// <summary>
    /// if provided, a delay of the provided Milliseconds will be added before proceeding like normal
    /// </summary>
    public int? DelayMs { get; set; }
    
    /// <summary>
    /// if provided, the normal underlying code will be skipped and this is the response that will be returned instead.
    /// </summary>
    public ActionRespondWith? RespondWith { get; set; }
}