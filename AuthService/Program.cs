using AuthService.Extensions;
using AuthService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Register all services (DI) via extensions
builder.Services.AddAuthApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure pipeline via middlewares
app.UseAuthPipeline();

app.Run();
