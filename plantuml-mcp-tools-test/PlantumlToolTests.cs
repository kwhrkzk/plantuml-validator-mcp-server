using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions; // Add this namespace
using PlantumlTools;

public class PlantumlMcpToolTests
{
    private readonly PlantumlTool _plantumlTool;
    private readonly ITestOutputHelper _output; // Add this field

    public PlantumlMcpToolTests(ITestOutputHelper output) // Add parameter
    {
        _output = output; // Assign output helper
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config["PlantumlBaseUrl"]).Returns("http://10.3.0.131:8099/");
        var logger = new XunitLogger<PlantumlTool>(output); // Use XunitLogger
        _plantumlTool = new PlantumlTool(mockConfiguration.Object, logger); // Pass logger
    }

    [Fact]
    public async Task ValidatePlantUml_ValidMessage_ReturnsOk()
    {
        // Arrange
        var message = "@startuml\nAlice -> Bob: Hello\n@enduml";

        // Act
        var result = await _plantumlTool.ValidatePlantuml(message);

        _output.WriteLine(result); // Use ITestOutputHelper

        // Assert
        Assert.Equal("Ok", result);
    }

    [Fact]
    public async Task ValidatePlantUml_InvalidMessage_ReturnsErrorDetails()
    {
        // Arrange
        var message = "@startuml\nclass A\nclass B\nA <|-a- B\n@enduml"; // Syntax error
        _output.WriteLine(message); // Log the message

        // Act
        var result = await _plantumlTool.ValidatePlantuml(message);

        _output.WriteLine(result); // Use ITestOutputHelper

        // Assert
        Assert.Contains("FAIL", result);
    }
}
