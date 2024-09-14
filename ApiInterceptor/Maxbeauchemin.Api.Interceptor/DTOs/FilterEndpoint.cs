namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class FilterEndpoint
{
    /// <summary>
    /// type of method to match, can be wild-carded with "*"
    /// </summary>
    public string MethodType { get; set; }
    
    /// <summary>
    /// URL to match, can be partial, or can be wild-carded with "*"
    /// </summary>
    public string URL { get; set; }
    
    /// <summary>
    /// query parameters to match, each key can have multiple values to attempt to match to, but the request must contain a match in each key
    /// </summary>
    public List<EndpointParameters>? Parameters { get; set; }
    
    /// <summary>
    /// request body properties to match, each key can have multiple values to attempt to match to, but the request must contain a match in each key
    ///
    /// Note: currently only supports JSON request body
    /// </summary>
    public List<EndpointBodyProperty>? BodyProperties { get; set; }
}