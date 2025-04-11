using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using PlantumlTools;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Configuration.AddCommandLine(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<PlantumlTool>();

await builder.Build().RunAsync();
