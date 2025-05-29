using ManualGateway.Api.Services;
using Prometheus;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Services.AddControllers();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .LoadFromMemory([], []);
builder.Services.AddScoped<SystemRoutingRepository>();

var app = builder.Build();

// At startup (before any HTTP requests), there's no active scope. Scoped services can only be resolved
// within an active scope context.
using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<SystemRoutingRepository>();
    var initialRoutes = GetRoutesFromDatabase();
    var initialClusters = await GetClustersFromDatabase(repository);

    var configProvider = app.Services.GetRequiredService<InMemoryConfigProvider>();
    configProvider.Update(initialRoutes, initialClusters);
}

app.Map("/update", async context =>
{
    var repository = context.RequestServices.GetRequiredService<SystemRoutingRepository>();
    var routes = GetRoutesFromDatabase();
    var clusters = await GetClustersFromDatabase(repository);
    context.RequestServices.GetRequiredService<InMemoryConfigProvider>().Update(routes, clusters);
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

// Use and send metrics to Prometheus
app.UseHttpMetrics();
app.MapMetrics();

app.Run();

RouteConfig[] GetRoutesFromDatabase()
{
    return
    [
        new RouteConfig
        {
            RouteId = "product-route",
            ClusterId = "product-cluster",
            Match = new RouteMatch
            {
                Path = "/api/products/{**catch-all}"
            }
        }
    ];
}

async Task<ClusterConfig[]> GetClustersFromDatabase(SystemRoutingRepository repository)
{
    var systemRoutes = await repository.GetAllRoutesAsync();
    var destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);

    foreach (var systemRoute in systemRoutes)
    {
        destinations.Add(
            $"product-dest-{systemRoute.SystemId}",
            new DestinationConfig { Address = systemRoute.ProductServiceTarget }
        );
    }

    return
    [
        new ClusterConfig()
        {
            ClusterId = $"product-cluster",
            Destinations = destinations
        }
    ];

}

/// <summary>
/// Custom proxy step that filters destinations based on a header in the inbound request
/// </summary>
Task MyCustomProxyStep(HttpContext context, Func<Task> next)
{
    // Can read data from the request via the context
    var destinationHeaderPresent = context.Request.Headers.TryGetValue("destination", out var headerValues) && headerValues.Count == 1;
    var destination = headerValues.FirstOrDefault();

    // The context also stores a ReverseProxyFeature which holds proxy specific data such as the cluster, route and destinations
    var availableDestinationsFeature = context.Features.Get<IReverseProxyFeature>();

    if (!destinationHeaderPresent || destination is null || availableDestinationsFeature is null)
    {
        context.Response.StatusCode = 400;
        context.Response.WriteAsync("Destination header not present. Cannot route the request.");
        return Task.CompletedTask;
    }
    var filteredDestinations = availableDestinationsFeature.AvailableDestinations
        .Where(d => d.DestinationId.Contains(destination)).ToList();

    availableDestinationsFeature.AvailableDestinations = filteredDestinations;

    // Important - required to move to the next step in the proxy pipeline
    return next();
}