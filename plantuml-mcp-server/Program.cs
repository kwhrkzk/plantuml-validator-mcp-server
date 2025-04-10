var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddCommandLine(args);

builder.Services.AddMcpServer()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run("http://0.0.0.0:3000");