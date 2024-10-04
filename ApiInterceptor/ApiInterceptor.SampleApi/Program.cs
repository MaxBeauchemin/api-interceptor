using System.Net;
using Maxbeauchemin.Api.Interceptor.DTOs;
using Maxbeauchemin.Api.Interceptor.Filters;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);

#region API Interceptor Setup

var apiInterceptorOptions = new Options
{
    Enabled = true,
    Scenarios = new List<Scenario>
    {
        new ()
        {
            Name = "SampleScenario",
            Enabled = true,
            Filter = new ()
            {
                Identities = new List<string> { "TestIdentity" },
                Endpoints = new List<FilterEndpoint>
                {
                    new ()
                    {
                        MethodType = "GET",
                        URL = "*"
                    },
                    new ()
                    {
                        MethodType = "POST",
                        URL = "*",
                        BodyProperties = new List<EndpointBodyProperty>
                        {
                            new ()
                            {
                                Path = "$.X.Y",
                                Values = [ "Sample" ]
                            }
                        }
                    }
                }
            },
            Actions = new ()
            {
                RespondWith = new ()
                {
                    HttpCode = HttpStatusCode.Gone,
                    Body = new {
                        InterceptedBody = true
                    }
                }
            }
        }
    }
};

var identityProvider = (ActionExecutingContext ctx) =>
{
    var identityHeader = ctx.HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "X-Identity");

    return identityHeader.Value.ToString();
};

using ILoggerFactory logFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = logFactory.CreateLogger("API Interceptor");

var filter = new ApiInterceptorFilterAttribute(apiInterceptorOptions, identityProvider, logger);

filter.Order = int.MinValue;
    
builder.Services.AddMvc(opts =>
{
    opts.Filters.Add(filter);
});

#endregion

var app = builder.Build();

app.Use(next => context =>
{
    context.Request.EnableBuffering();
    return next(context);
});

app.MapControllers();

app.Run();

//To test this code, you can run the app and call the following command from your terminal window
//  curl http://localhost:5153/api/v1/Sample/Test?input=sample
//then you can call this to see it getting intercepted
//  curl http://localhost:5153/api/v1/Sample/Test?input=sample --header "X-Identity: TestIdentity"
//or this one
//  curl http://localhost:5153/api/v1/Sample/Test --header "X-Identity: TestIdentity" --data "{ \"X\": { \"Y\": \"Sample\" } }"