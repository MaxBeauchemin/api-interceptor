using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;
using Maxbeauchemin.Api.Interceptor.DTOs;
using Maxbeauchemin.Api.Interceptor.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Maxbeauchemin.Api.Interceptor.Filters;

public class ApiInterceptorFilterAttribute : ActionFilterAttribute
{
    private Func<Options> _optionsProvider;
    private Func<ActionExecutingContext, string?>? _identityProvider;
    private ILogger? _logger;
    private readonly Random _random;
    
    private const string CustomResponseHeader = "X-Api-Interceptor-Scenario";

    public ApiInterceptorFilterAttribute(Options options, Func<ActionExecutingContext, string?>? identityProvider = null, ILogger? logger = null)
    {
        _optionsProvider = () => options;
        _identityProvider = identityProvider;
        _logger = logger;
        _random = new Random();
    }
    
    public ApiInterceptorFilterAttribute(Func<Options> optionsProvider, Func<ActionExecutingContext, string?>? identityProvider = null, ILogger? logger = null)
    {
        _optionsProvider = optionsProvider;
        _identityProvider = identityProvider;
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Updates the Options Provider in use
    /// </summary>
    /// <param name="optionsProvider"></param>
    public void SetOptionsProvider(Func<Options> optionsProvider)
    {
        _optionsProvider = optionsProvider;
    }

    /// <summary>
    /// Updates the Identity Provider in use
    /// </summary>
    /// <param name="identityProvider"></param>
    public void SetIdentityProvider(Func<ActionExecutingContext, string?> identityProvider)
    {
        _identityProvider = identityProvider;
    }

    /// <summary>
    /// Updates the Logger in use
    /// </summary>
    /// <param name="logger"></param>
    public void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            await CheckScenarios(context);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error occured while processing API Interceptor code... Details: {ex.Message}. StackTrace: {ex.StackTrace}");
        }

        if (context.Result == null) await next();
    }

    private async Task CheckScenarios(ActionExecutingContext context)
    {
        var options = _optionsProvider();

        if (!options.Enabled) return;

        var enabledScenarios = options.Scenarios.Where(s => s.Enabled).ToList();

        if (!enabledScenarios.Any()) return;

        var identity = _identityProvider != null ? _identityProvider(context) : null;
        var methodType = context.HttpContext.Request.Method;
        var url = context.HttpContext.Request.Path.Value;
        var queryString = context.HttpContext.Request.QueryString.Value;
        var queryParams = context.HttpContext.Request.Query?
            .ToDictionary(q => q.Key.ToLower().Trim(), q => q.Value.Select(v => v.Trim()).ToList()) ?? new ();

        var anyScenariosUseBody = enabledScenarios.Any(s => s.Filter.Endpoints?.Any(e => e.BodyProperties != null && e.BodyProperties.Any()) ?? false);

        var body = anyScenariosUseBody ? await GetBody(context.HttpContext.Request, _logger) : null;

        var scenarioMatch = enabledScenarios.Find(scenario => MatchesScenario(scenario.Filter, identity, methodType, url, queryParams, body));

        if (scenarioMatch == null) return;

        if (!context.HttpContext.Response.Headers.ContainsKey(CustomResponseHeader))
        {
            context.HttpContext.Response.Headers.Add(CustomResponseHeader, scenarioMatch.Name);
        }
        
        if (!scenarioMatch.DisableWarningLog)
        {
            _logger?.LogWarning($"API Intercepted for Scenario {scenarioMatch.Name} - Identity: {identity ?? "<NONE>"}, {methodType} {url} {queryString}");
        }

        if (scenarioMatch.Actions.DelayMs != null)
        {
            Thread.Sleep(scenarioMatch.Actions.DelayMs.Value);
        }
        
        if (scenarioMatch.Actions.RespondWith != null)
        {
            ShortCircuitResponse(context, scenarioMatch.Actions.RespondWith);
        }
    }

    private bool MatchesScenario(ScenarioFilter filter, string? identity, string methodType, string url, Dictionary<string, List<string>> queryParams, string? body)
    {
        var matchesEmails = MatchesIdentity(filter, identity);

        var matchesEndpoint = MatchesEndpoint(filter, methodType, url, queryParams, body);

        var matchesPercentage = MatchesPercentage(filter);
        
        return matchesEmails && matchesEndpoint && matchesPercentage;
    }

    private bool MatchesIdentity(ScenarioFilter filter, string? identity)
    {
        if (filter.Identities != null && filter.Identities.Any())
        {
            if (identity == null) return false;
            
            return filter.Identities.Exists(i => i.Trim().Equals(identity.Trim(), StringComparison.InvariantCultureIgnoreCase));
        }

        return true;
    }

    private static bool MatchesEndpoint(ScenarioFilter filter, string methodType, string url, Dictionary<string, List<string>> queryParams, string? body)
    {
        if (filter.Endpoints != null && filter.Endpoints.Any())
        {
            foreach (var e in filter.Endpoints)
            {
                var matchingMethodType = e.MethodType.Trim() == "*" ||
                                         e.MethodType.Trim().ToLower() == methodType.ToLower();

                var matchingUrl = e.URL.Trim() == "*" ||
                                  url.ToLower().Contains(e.URL.Trim().ToLower());

                var matchingParameters = MatchesParameters(e, queryParams);

                var matchingBody = MatchesBody(e, body);

                if (matchingMethodType && matchingUrl && matchingParameters && matchingBody) return true;
            }

            return false;
        }

        return true;
    }

    private static bool MatchesParameters(FilterEndpoint endpoint, Dictionary<string, List<string>> queryParams)
    {
        if (endpoint.Parameters == null || endpoint.Parameters.Count == 0) return true;

        foreach (var p in endpoint.Parameters)
        {
            var key = p.Key.ToLower().Trim();

            if (!queryParams.TryGetValue(key, out var paramValues)) return false;

            var matched = p.Values.Intersect(paramValues).Any();

            if (!matched) return false;
        }

        return true;
    }

    private static bool MatchesBody(FilterEndpoint endpoint, string? body)
    {
        if (endpoint.BodyProperties == null || endpoint.BodyProperties.Count == 0) return true;

        if (body == null) return false;

        var instance = JsonNode.Parse(body);

        if (instance == null) return false;
        
        foreach (var p in endpoint.BodyProperties)
        {
            if (!JsonMatchUtility.MatchesJson(instance, p.Path, p.Values)) return false;
        }

        return true;
    }

    private bool MatchesPercentage(ScenarioFilter filter)
    {
        if (filter.Percentage == null) return true;

        if (filter.Percentage <= 0) return false;

        if (filter.Percentage >= 100) return true;

        return _random.NextInt64(100) <= filter.Percentage;
    }
    
    private static void ShortCircuitResponse(ActionExecutingContext context, ActionRespondWith respondWith)
    {
        var responseBodyJson = respondWith.Body != null ? JsonSerializer.Serialize(respondWith.Body) : "{}";

        var result = new ContentResult
        {
            Content = responseBodyJson,
            ContentType = MediaTypeNames.Application.Json,
            StatusCode = (int)respondWith.HttpCode
        };
        
        context.Result = result;
    }

    private static async Task<string?> GetBody(HttpRequest request, ILogger? logger)
    {
        if (request.Body == null) return null;
        
        if (!request.Body.CanSeek)
        {
            if (logger != null) logger.LogWarning("Request Buffering has not been enabled - cannot read Request Body");
            return null;
        }

        request.Body.Position = 0;

        var reader = new StreamReader(request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);

        request.Body.Position = 0;

        return body;
    }
}
