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
}