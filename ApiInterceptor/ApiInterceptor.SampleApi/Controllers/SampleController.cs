using Microsoft.AspNetCore.Mvc;

namespace ApiInterceptor.SampleApi.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SampleController : ControllerBase
{
    [HttpGet]
    [Route("Test")]
    public object SampleTest()
    {
        return new
        {
            RealPayload = true
        };
    }
}