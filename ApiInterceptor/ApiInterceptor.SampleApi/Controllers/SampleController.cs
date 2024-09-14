using Microsoft.AspNetCore.Mvc;

namespace ApiInterceptor.SampleApi.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SampleController : ControllerBase
{
    [HttpGet]
    [Route("Test")]
    public object SampleGet([FromQuery]string input)
    {
        return new
        {
            RealPayload = true,
            RequestQueryParameter = input
        };
    }

    [HttpPost]
    [Route("Test")]
    public object SamplePost([FromBody]object request)
    {
        return new
        {
            RealPayload = true,
            RequestBody = request
        };
    }
}