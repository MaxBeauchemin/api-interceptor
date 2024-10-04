using System.Net;
using System.Text.Json;
using Maxbeauchemin.Api.Interceptor.DTOs;
using Maxbeauchemin.Api.Interceptor.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using static System.Formats.Asn1.AsnWriter;

namespace ApiInterceptor.Tests;

public class ApiInterceptorFilterAttributeTests
{
    private static ActionExecutingContext GetMockActionContext(string methodType, string url, QueryCollection queryCollection = default, string body = "")
    {
        var mockHttpContext = new Mock<HttpContext>();
        
        var mockHttpRequest = new Mock<HttpRequest>();

        mockHttpRequest.SetupProperty(m => m.Method, methodType);
        mockHttpRequest.SetupProperty(m => m.Path, new PathString(url));
        mockHttpRequest.SetupProperty(m => m.Query, queryCollection);

        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write(body);
        streamWriter.Flush();

        mockHttpRequest.SetupProperty(m => m.Body, memoryStream);

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

    [Theory]
    [InlineData("input", true)]
    [InlineData("test", false)]
    public void QueryParamsFilter(string key, bool shouldIntercept)
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
                                URL = "*",
                                Parameters = new List<EndpointParameters>
                                {
                                    new ()
                                    {
                                        Key = key,
                                        Values = [ "a", "b", "c" ]
                                    }
                                }
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

        var queryItems = new Dictionary<string, StringValues>
        {
            { "input", "b" }
        };

        var context = GetMockActionContext("GET", "/api/v1/Sample", queryCollection: new QueryCollection(queryItems));

        var filter = BuildFilter(options);

        //Act

        filter.OnActionExecuting(context);

        //Assert

        Assert.Null(context.Result);

        var interceptedHeader = context.HttpContext.Response.Headers.FirstOrDefault(h => h.Key == "X-Api-Interceptor-Scenario");

        Assert.Equal(shouldIntercept ? "TestIntercepted" : null, interceptedHeader.Value);
    }

    [Theory]
    [InlineData("$.X", true)]
    [InlineData("$.Y", false)]
    public void BodyFilter(string path, bool shouldIntercept)
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
                                MethodType = "post",
                                URL = "*",
                                BodyProperties = new List<EndpointBodyProperty>
                                {
                                    new ()
                                    {
                                        Path = path,
                                        Values = [ "1", "2", "3" ]
                                    }
                                }
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

        var context = GetMockActionContext("POST", "/api/v1/Sample", body: JsonSerializer.Serialize(new { X = 2 }));

        var filter = BuildFilter(options);

        //Act

        filter.OnActionExecuting(context);

        //Assert

        Assert.Null(context.Result);

        var interceptedHeader = context.HttpContext.Response.Headers.FirstOrDefault(h => h.Key == "X-Api-Interceptor-Scenario");

        Assert.Equal(shouldIntercept ? "TestIntercepted" : null, interceptedHeader.Value);
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