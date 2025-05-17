using System.Diagnostics;
using Gateway.Api.Services; // For SystemRoutingRepository
using Yarp.ReverseProxy.Forwarder; // For IHttpForwarder
using System.Net; // For DecompressionMethods, SocketsHttpHandler

var builder = WebApplication.CreateBuilder(args);

// 1. Load YARP static configuration (for inventory, orders, and fallback)
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);

// 2. Add YARP services and load static config
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 3. Add our custom SystemRoutingRepository
builder.Services.AddScoped<SystemRoutingRepository>(); // Scoped is fine

// 4. Add IHttpForwarder for manual forwarding
builder.Services.AddHttpForwarder();

var app = builder.Build();

// 5. Prepare HttpClient for IHttpForwarder
// This configuration is recommended by YARP docs for direct forwarding.
var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current), // For distributed tracing
    ConnectTimeout = TimeSpan.FromSeconds(15),
});
// Distributed Tracing Context: In a microservices architecture, a single user request might flow through multiple services
// (e.g., Gateway -> OrderService -> InventoryService). To trace the entire lifecycle of such a request across these services,
// a "trace context" (containing IDs like a Trace ID and Span ID) needs to be passed from one service to the next.
// W3C Trace Context / OpenTelemetry: The standard for propagating this context is often the W3C Trace Context specification,
// which defines specific HTTP headers (like traceparent and tracestate) to carry this information. OpenTelemetry is a widely
// adopted observability framework that implements these standards.
// ActivitySource and Activity: In .NET, distributed tracing is often implemented using System.Diagnostics.ActivitySource
// and System.Diagnostics.Activity. An Activity represents a unit of work and carries the trace context.
// DistributedContextPropagator: This is a .NET class responsible for injecting (serializing) the current Activity's context
// into outgoing HTTP request headers and extracting (deserializing) it from incoming HTTP request headers.
// DistributedContextPropagator.Current usually defaults to a propagator that handles W3C Trace Context headers.
// ReverseProxyPropagator: This is a YARP-specific wrapper. When YARP (acting as the IHttpForwarder) makes an outgoing request
// to a backend service, this propagator ensures that if there's an active Activity (trace context) on the incoming request
// to the gateway, its context is correctly propagated (i.e., the right headers like traceparent are added) to the outgoing request
// being sent to the backend service.
// Why SocketsHttpHandler needs it: The SocketsHttpHandler (which is the underlying handler for HttpClient / HttpMessageInvoker)
// doesn't automatically know about Activity propagation in the same way that the higher-level HttpClientFactory or ASP.NET Core's
// built-in client might. By setting the ActivityHeadersPropagator explicitly, we ensure that this httpClient instance used by
// IHttpForwarder participates correctly in distributed tracing.


// Get IHttpForwarder and ILogger for use in route handlers
var forwarder = app.Services.GetRequiredService<IHttpForwarder>();
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("DynamicProductRouting");

// 6. CUSTOM DYNAMIC ROUTING for ProductService
// This needs to be mapped *before* app.MapReverseProxy() if we want to intercept these paths.
app.Map("/products-api/{**rest}", async (HttpContext httpContext, string rest) =>
{
    // Try to get 'system' query parameter
    if (!httpContext.Request.Query.TryGetValue("system", out var systemIdValues) || string.IsNullOrEmpty(systemIdValues.FirstOrDefault()))
    {
        logger.LogInformation("Product route: 'system' query parameter missing or empty. Falling back to static YARP config.");
        // If 'system' query parameter is missing, we can either:
        // 1. Return a BadRequest
        // httpContext.Response.StatusCode = 400;
        // await httpContext.Response.WriteAsync("'system' query parameter is required for /products-api routes.");
        // return;

        // 2. Or, let it fall through to be handled by the static yarp.json config (if a route matches there)
        // To do this, we essentially re-invoke the YARP pipeline for this specific context.
        // This is a bit more advanced; for now, a simple fallback or error is easier.
        // For simplicity, let's assume if no 'system' is passed, we don't dynamically route here.
        // The request *might* be caught by app.MapReverseProxy() later if a general /products-api route exists there.
        // Let's explicitly say we require it for this dynamic path for now.
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsync("The 'system' query parameter is required for this product API endpoint.");
        return;
    }

    var systemId = systemIdValues.First();
    var routingRepo = httpContext.RequestServices.GetRequiredService<SystemRoutingRepository>(); // Get repo from DI
    logger.LogInformation("Product route: Attempting dynamic route for systemId '{SystemId}' and path '/{Rest}'", systemId, rest);

    var systemRoute = await routingRepo.GetRouteBySystemIdAsync(systemId!);
    if (systemRoute == null || string.IsNullOrEmpty(systemRoute.ProductServiceTarget))
    {
        logger.LogWarning("Product route: No specific route found for systemId '{SystemId}' in database, or ProductServiceTarget is missing.", systemId);
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsync($"No product routing configuration found for system '{systemId}'.");
        return;
    }

    var productRoute = systemRoute.ProductServiceTarget;

    logger.LogInformation("Product route: Forwarding to '{SystemRoute}/{Rest}' for systemId '{SystemId}'", productRoute, rest, systemId);

    // Forward the request. YARP takes care of copying headers, body, etc.
    // The 'rest' variable already includes the leading '/' if it was part of the original path after /products-api/
    // We need to ensure 'rest' correctly forms the path *after* the hostname.
    // If 'rest' starts with '/', destinationPrefix should not have a trailing '/'.
    // If 'rest' doesn't start with '/', destinationPrefix might need a trailing '/' or 'rest' needs a leading '/'.
    // The {**rest} captures segments including slashes.
    
    // Let's use the 'rest' parameter directly.
    // If original is /products-api/foo/bar then 'rest' is foo/bar
    // We need to send to http://target:8080/foo/bar
    // So, the path to forward is simply 'rest' if YARP expects the full path after hostname.
    // However, IHttpForwarder.SendAsync wants the full URI.

    // Let's rebuild the path for the backend:
    var backendPath = $"/{rest}{httpContext.Request.QueryString}";

    // This replaces the current request's path with the new one for forwarding
    // This is a simplified way; YARP's transforms offer more robust path manipulation.
    // For IHttpForwarder, we simply provide the full target URI.

    var targetUri = $"{productRoute}{backendPath}";
    logger.LogInformation("Product route: Constructed Target URI '{TargetUri}'", targetUri);

    var error = await forwarder.SendAsync(httpContext, targetUri, httpClient);
    if (error != ForwarderError.None)
    {
        var errorFeature = httpContext.GetForwarderErrorFeature();
        var exception = errorFeature?.Exception;
        logger.LogError(exception, "Product route: Error forwarding request for systemId '{SystemId}'. Error: {ForwarderError}", systemId, error);
        // Error handling (e.g., return 502 Bad Gateway or specific error)
        if (!httpContext.Response.HasStarted) // Check if response has already started
        {
            httpContext.Response.StatusCode = StatusCodes.Status502BadGateway;
            await httpContext.Response.WriteAsync("Error forwarding request to backend product service.");
        }
    }
    else
    {
        logger.LogInformation("Product route: Successfully forwarded request for systemId '{SystemId}' to '{TargetUri}'", systemId, targetUri);
    }
});


// 7. Map YARP reverse proxy middleware for all other routes (inventory, orders, or product fallback)
// This will use the routes defined in yarp.json
app.MapReverseProxy();

app.Run();
