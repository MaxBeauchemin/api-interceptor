using System.Net.Mime;
using System.Text.Json;
using Maxbeauchemin.Api.Interceptor.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Maxbeauchemin.Api.Interceptor.Filters;

public class ApiInterceptorFilterAttribute : ActionFilterAttribute
{
    private readonly Func<Options> _optionsProvider;
    private readonly Func<ActionExecutingContext, string?>? _identityProvider;
    private readonly ILogger? _logger;
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

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            CheckScenarios(context);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error occured while processing API Interceptor code... Details: {ex.Message}. StackTrace: {ex.StackTrace}");
        }
    }

    private void CheckScenarios(ActionExecutingContext context)
    {
        var options = _optionsProvider();

        if (!options.Enabled) return;

        var enabledScenarios = options.Scenarios.Where(s => s.Enabled).ToList();

        if (!enabledScenarios.Any()) return;

        var identity = _identityProvider != null ? _identityProvider(context) : null;
        var methodType = context.HttpContext.Request.Method;
        var url = context.HttpContext.Request.Path.Value;
        var queryString = context.HttpContext.Request.QueryString.Value;

        var queryParams = context.HttpContext.Request.Query
            .ToDictionary(q => q.Key.ToLower().Trim(), q => q.Value.Select(v => v.Trim()).ToList());
        
        var scenarioMatch = enabledScenarios.Find(scenario => MatchesScenario(scenario.Filter, identity, methodType, url, queryParams));

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

    private bool MatchesScenario(ScenarioFilter filter, string? identity, string methodType, string url, Dictionary<string, List<string>> queryParams)
    {
        var matchesEmails = MatchesIdentity(filter, identity);

        var matchesEndpoint = MatchesEndpoint(filter, methodType, url, queryParams);

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

    private static bool MatchesEndpoint(ScenarioFilter filter, string methodType, string url, Dictionary<string, List<string>> queryParams)
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

                if (matchingMethodType && matchingUrl && matchingParameters) return true;
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
}