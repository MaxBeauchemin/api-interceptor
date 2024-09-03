using System.Net;
using Maxbeauchemin.Api.Interceptor.DTOs;
using Maxbeauchemin.Api.Interceptor.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace ApiInterceptor.Tests;

public class ApiInterceptorFilterAttributeTests
{
    private static ActionExecutingContext GetMockActionContext(string methodType, string url)
    {
        var mockHttpContext = new Mock<HttpContext>();
        
        var mockHttpRequest = new Mock<HttpRequest>();

        mockHttpRequest.SetupProperty(m => m.Method, methodType);
        mockHttpRequest.SetupProperty(m => m.Path, new PathString(url));
        mockHttpRequest.SetupProperty(m => m.QueryString, new QueryString("?test=true"));

        mockHttpContext.SetupGet(m => m.Request).Returns(mockHttpRequest.Object);

        var mockHttpResponse = new Mock<HttpResponse>();
        
        mockHttpResponse.SetupGet(m => m.Headers).Returns(new HeaderDictionary());
        
        mockHttpContext.SetupGet(m => m.Response).Returns(mockHttpResponse.Object);
        
        var actionContext = new ActionContext(mockHttpContext.Object, new Mock<RouteData>().Object, new Mock<ActionDescriptor>().Object);
        
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new { });
    }
    
    private ApiInterceptorFilterAttribute BuildFilter(Options options)
    {
        return new ApiInterceptorFilterAttribute(options, _ => "testIdentity");
    }
    
    [Fact]
    public void DisabledTest()
    {
        //Arrange
        var options = new Options
        {
            Enabled = false,
            Scenarios = new List<Scenario>()
        };
        
        var context = GetMockActionContext("GET", "/api/v1/Sample");
 
        var filter = BuildFilter(options);
        
        //Act
        
        filter.OnActionExecuting(context);
        
        //Assert
        
        Assert.Null(context.Result);
    }
    
    [Fact]
    public void IdentityOnlyFilter()
    {
        //Arrange
        var options = new Options
        {
            Enabled = true,
            Scenarios = new List<Scenario>
            {
                new ()
                {
                    Enabled = true,
                    Name = "TestIntercepted",
                    Filter = new ScenarioFilter
                    {
                        Identities = new List<string> { "testIdentity" }
                    },
                    Actions = new ScenarioActions
                    {
                        DelayMs = 100
                    }
                }
            }
        };

        var context = GetMockActionContext("GET", "/api/v1/Sample");

        var filter = BuildFilter(options);
        
        //Act
        
        filter.OnActionExecuting(context);
        
        //Assert
        
        Assert.Null(context.Result);

        var interceptedHeader = context.HttpContext.Response.Headers.FirstOrDefault(h => h.Key == "X-Api-Interceptor-Scenario");
        
        Assert.NotNull(interceptedHeader);
        Assert.Equal("TestIntercepted", interceptedHeader.Value);
    }
    
    [Fact]
    public void UrlOnlyFilter()
    {
        //Arrange
        var options = new Options
        {
            Enabled = true,
            Scenarios = new List<Scenario>
            {
                new ()
                {
                    Enabled = true,
                    Name = "TestIntercepted",
                    Filter = new ScenarioFilter
                    {
                        Endpoints = new List<FilterEndpoint>
                        {
                            new ()
                            {
                                MethodType = "get",
                                URL = "sample"
                            }
                        }
                    },
                    Actions = new ScenarioActions
                    {
                        DelayMs = 100
                    }
                }
            }
        };

        var context = GetMockActionContext("GET", "/api/v1/Sample");

        var filter = BuildFilter(options);
        
        //Act
        
        filter.OnActionExecuting(context);
        
        //Assert
        
        Assert.Null(context.Result);

        var interceptedHeader = context.HttpContext.Response.Headers.FirstOrDefault(h => h.Key == "X-Api-Interceptor-Scenario");
        
        Assert.NotNull(interceptedHeader);
        Assert.Equal("TestIntercepted", interceptedHeader.Value);
    }
    
    [Fact]
    public void RespondsWith()
    {
        //Arrange
        var options = new Options
        {
            Enabled = true,
            Scenarios = new List<Scenario>
            {
                new ()
                {
                    Enabled = true,
                    Name = "TestIntercepted",
                    Filter = new ScenarioFilter
                    {
                        Endpoints = new List<FilterEndpoint>
                        {
                            new ()
                            {
                                MethodType = "get",
                                URL = "sample"
                            }
                        }
                    },
                    Actions = new ScenarioActions
                    {
                        RespondWith = new ActionRespondWith
                        {
                            HttpCode = HttpStatusCode.Forbidden,
                            Body = new
                            {
                                TestBody = true
                            }
                        }
                    }
                }
            }
        };

        var context = GetMockActionContext("GET", "/api/v1/Sample");

        var filter = BuildFilter(options);
        
        //Act
        
        filter.OnActionExecuting(context);
        
        //Assert
        
        var interceptedHeader = context.HttpContext.Response.Headers.FirstOrDefault(h => h.Key == "X-Api-Interceptor-Scenario");
        
        Assert.NotNull(interceptedHeader);
        Assert.Equal("TestIntercepted", interceptedHeader.Value);
        
        var contentResult = (ContentResult)context.Result;
        Assert.Equal(403, contentResult.StatusCode);
        Assert.Equal("{\"TestBody\":true}", contentResult.Content);
        Assert.Equal("application/json", contentResult.ContentType);
    }
}