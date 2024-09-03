namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class EndpointParameters
{
    /// <summary>
    /// the parameter name that must be provided
    /// </summary>
    public string Key { get; set; }
    
    /// <summary>
    /// the list of values that can be matched for this parameter
    /// </summary>
    public List<string> Values { get; set; }
}