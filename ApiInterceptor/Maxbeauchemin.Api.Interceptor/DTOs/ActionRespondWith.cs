using System.Net;

namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class ActionRespondWith
{
    /// <summary>
    /// the HTTP Status Code identifier to respond with
    /// </summary>
    public HttpStatusCode HttpCode { get; set; }
    
    /// <summary>
    /// if provided, this is the body that will be in the response. Defaults to `{}`
    /// </summary>
    public object? Body { get; set; }
}