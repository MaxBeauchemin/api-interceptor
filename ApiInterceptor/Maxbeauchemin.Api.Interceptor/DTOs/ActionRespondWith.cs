using System.Net;

namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class ActionRespondWith
{
    public HttpStatusCode HttpCode { get; set; }
    public object? Body { get; set; }
}