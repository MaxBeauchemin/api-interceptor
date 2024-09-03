# api-interceptor

The `Maxbeauchemin.Api.Interceptor` Nuget package allows you to create **scenarios** where your incoming API calls will be intercepted to help simulate outages or mock different behavior.

This package can be used to help test different error-states applications integrating with your code may be expected to endure.

## Usage

The main content of the package is the `ApiInterceptorFilterAttribute`

To enable the interceptor, you must either manually add the attribute to all the different API controllers that you want to be able to intercept using the `[ApiInterceptorFilterAttribute(..)]` syntax, or set it up globally (recommended)

For global setup, you can use the following syntax:
```csharp
var filter = new ApiInterceptorFilterAttribute(apiInterceptorOptions, identityProvider);

filter.Order = int.MinValue;
    
builder.Services.AddMvc(opts =>
{
    opts.Filters.Add(filter);
});
```

For this to work, you will need to create either an `Options` object, or a function that can be used to retrieve them when necessary.

The `Options` object configures the functionality of the interceptor and the different scenarios that it should be setup to handle. If you wish to be able to switch the options without having to restart your application, you will need to construct the class using an Options Provider function that can be used to retrieve the current setup.

You can optionally provide an Identity Provider which converts an `ActionExecutingContext` into a String identifying a specific actor. This would normally be used to extract user-identity information from an API token, but is left flexible for your implementation.

Here's an example of an Identity Provider which would get identity information from an API header:

```csharp
var identityProvider = (ActionExecutingContext ctx) =>
{
    var identityHeader = ctx.HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "X-Identity");

    return identityHeader.Value.ToString();
};
```

If the Options Provider / Identity Provider / Logger aren't able to be set at program startup, you can swap them out later using their respective `Set` functions, as long as you maintain a reference to the Filter class you initialized.

## Options

This section describes the different parameters that can be provided in the Options objects to control the Api Interceptor functionality

- `Enabled (bool req.)` - whether the API interceptor should be enabled
- `Scenarios (array req.)` - the list of scenarios that should be handled. The order listed is the order they will be checked in. The first matching scenario for a request is the only one that will be used (if any).
  - `Name (string req.)` - name of the scenario, will be attached as an `X-Api-Interceptor-Scenario` response header and used for Warning Logs
  - `Enabled (bool req.)` - whether this individual scenario should be checked
  - `DisableWarningLog (bool opt.)` - whether Warning logs should be disabled. Default is all interceptions record warning logs.
  - `Filter (object req.)` - the criteria that will be used to identify if a particular request should be intercepted by this scenario
    - `Identities (string array opt.)` - if provided, only identities (obtained from the identity provider) that are in the list will be intercepted
    - `Endpoints (array opt.)` - if provided, only APIs that match one of the items in this collection will be intercepted
      - `MethodType (string req.)` - type of method to match, can be wild-carded with `*`
      - `URL (string req.)` - URL to match, can be partial, or can be wild-carded with `*`
      - `Parameters (array opt.)` - query parameters to match, each key can have multiple values to attempt to match to, but the request must contain a match in each key
        - `Key (string req.)` - the parameter name that must be provided
        - `Values (string array req.)` - the list of values that can be matched for this parameter
    - `Percentage (int opt.)` - if provided, a random integer between `1` and `100` will be chosen, and the request will only be intercepted if the number is lower than this parameter's value
  - `Actions (object req.)` - the action(s) that will be performed if this scenario intercepts a request
    - `DelayMs (int opt.)` - if provided, a delay of the provided Milliseconds will be added before proceeding like normal
    - `RespondWith (object opt.)` - if provided, the normal underlying code will be skipped and this is the response that will be returned instead.
      - `HttpCode (int req.)` - the HTTP Status Code identifier to respond with
      - `Body (object opt.)` - if provided, this is the body that will be in the response. Defaults to `{}`