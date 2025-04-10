using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using PlantUmlTools;

public class PlantumlToolTests
{
    [Fact]
    public async Task CreatePng_ValidMessage_ReturnsBase64String()
    {
        // Arrange
        var message = "@startuml\nAlice -> Bob: Hello\n@enduml";
        var expectedBase64Pattern = @"^[a-zA-Z0-9+/]*={0,2}$"; // Base64 regex pattern

        // Act
        var result = await PlantumlTool.CreatePng(message);

        // Assert
        Assert.Matches(expectedBase64Pattern, result);
    }

    [Fact]
    public async Task ValidatePlantUml_ValidMessage_ReturnsOk()
    {
        // Arrange
        var message = "@startuml\nAlice -> Bob: Hello\n@enduml";

        // Act
        var result = await PlantumlTool.ValidatePlantUml(message);

        // Assert
        Assert.Equal("Ok", result);
    }

    [Fact]
    public async Task ValidatePlantUml_InvalidMessage_ReturnsErrorDetails()
    {
        // Arrange
        var message = "@startuml\npackage \"Docker Compose\" {\n  component myplantuml as \"PlantUML Server\" <<container>> {\n    note right: Image: plantuml/plantuml-server:v1.2025.2\n  }\n  component plantuml_mcp_server as \"PlantUML MCP Server\" <<container>> {\n    note right: Image: mcr.microsoft.com/dotnet/sdk:9.0\nVolume: .:/workspace\nWorking Dir: /workspace\nPorts: 3000:3000\nCommand: dotnet run --project plantuml-mcp-server/plantuml-mcp-server.csproj\n  }\n}\nplantuml_mcp_server -> myplantuml : Communicates via port 8080\n@enduml"; // Syntax error

        // Act
        var result = await PlantumlTool.ValidatePlantUml(message);

        // Assert
        Assert.Contains("FAIL", result);
    }
}
