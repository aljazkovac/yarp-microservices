var builder = WebApplication.CreateBuilder(args);

// 1. Load YARP configuration from yarp.json 
// Alternatively, YARP configuration can be directly in appsettings.json or appsettings.Development.json
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);

// 2. Add YARP services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")); // Tells YARP to load config from the "ReverseProxy" section

var app = builder.Build();

// 3. Map YARP reverse proxy middleware
// This should typically be one of the last things in the pipeline before Run()
// if you don't have other specific middleware that needs to run after it.
app.MapReverseProxy();

app.Run();