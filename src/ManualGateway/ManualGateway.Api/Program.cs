using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Services.AddControllers();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .LoadFromMemory(GetRoutes(), GetClusters());

var app = builder.Build();

app.Map("/update", context =>
{
    context.RequestServices.GetRequiredService<InMemoryConfigProvider>().Update(GetRoutes(), GetClusters());
    return Task.CompletedTask;
});
// We can customize the proxy pipeline and add/remove/replace steps
app.MapReverseProxy(proxyPipeline =>
{
    // Use a custom proxy middleware, defined below
    proxyPipeline.Use(MyCustomProxyStep);
    // Don't forget to include these two middleware when you make a custom proxy pipeline (if you need them).
    proxyPipeline.UseSessionAffinity();
    proxyPipeline.UseLoadBalancing();
});

app.Run();

RouteConfig[] GetRoutes()
{
    return
    [
        new RouteConfig()
        {
            RouteId = "route" + Random.Shared.Next(), // Forces a new route id each time GetRoutes is called.
            ClusterId = "cluster1",
            Match = new RouteMatch
            {
                // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                Path = "{**catch-all}"
            }
        }
    ];
}

ClusterConfig[] GetClusters()
{
    return
    [
        new ClusterConfig
        {
            ClusterId = "cluster1",
            SessionAffinity = new SessionAffinityConfig { Enabled = true, Policy = "Cookie", AffinityKeyName = ".Yarp.ReverseProxy.Affinity" },
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                { "destination1", new DestinationConfig() { Address = "https://example.com" } },
                { "destination2", new DestinationConfig() { Address = "https://bing.com" } },
            }
        }
    ];
}

/// <summary>
/// Custom proxy step that filters destinations based on a header in the inbound request
/// Looks at each destination metadata, and filters in/out based on their debug flag and the inbound header
/// </summary>
Task MyCustomProxyStep(HttpContext context, Func<Task> next)
{
    // Can read data from the request via the context
    var destination = context.Request.Headers.TryGetValue("Destination", out var headerValues) && headerValues.Count == 1;

    // The context also stores a ReverseProxyFeature which holds proxy specific data such as the cluster, route and destinations
    var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();

    // Important - required to move to the next step in the proxy pipeline
    return next();
}
