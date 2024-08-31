using System.Net;

namespace ApiInterceptor.DTOs;

public class ActionRespondWith
{
    public HttpStatusCode HttpCode { get; set; }
    public object? Body { get; set; }
}