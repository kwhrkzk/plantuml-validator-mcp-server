using System.ComponentModel;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace PlantumlTools
{
    [McpServerToolType]
    public class PlantumlTool
    {
        private Uri PlantumlBaseUrl { get; }
        private readonly ILogger<PlantumlTool> _logger;
        public PlantumlTool(IConfiguration configuration, ILogger<PlantumlTool> logger)
        {
            var baseUrl = configuration["PlantumlBaseUrl"];
            PlantumlBaseUrl = !string.IsNullOrEmpty(baseUrl) ? new Uri(baseUrl) : new Uri("http://myplantuml:8080/");
            _logger = logger;
            _logger.LogInformation($"Plantuml Base URL: {PlantumlBaseUrl}");
        }

#if DEBUG
        [McpServerTool, Description("connection test. echo message.")]
        public string Echo(string message)
        {
            return $"Hello {message}";
        }
#endif

#if DEBUG
        [McpServerTool, Description("Generates a PNG image from the provided Plantuml message and returns it as a Base64 string.")]
#endif
        public async Task<string> CreatePng(string message)
        {
            var encodedMessage = EncodePlantuml(message);
            Uri url = new(PlantumlBaseUrl, $"png/{encodedMessage}");

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var imageBytes =
             await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(imageBytes);
        }

        [McpServerTool, Description("Validates the provided Plantuml message. If valid, returns 'Ok'. If invalid, returns detailed error information including error description, error line, and other metadata.")]
        public async Task<string> ValidatePlantuml(string message)
        {
            try
            {
                var encodedMessage = EncodePlantuml(message);
                Uri url = new(PlantumlBaseUrl, $"txt/{encodedMessage}");
                _logger.LogInformation($"Validating Plantuml: {url}");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
                _logger.LogInformation($"Response Headers: {string.Join(", ", headers.Select(h => $"{h.Key}: {h.Value}"))}");

                if (!headers.TryGetValue("X-PlantUML-Diagram-Description", out var description) || !description.Contains("Error"))
                {
                    return "Ok";
                }

                var errorDetails = new StringBuilder();
                errorDetails.AppendLine("FAIL");
                errorDetails.AppendLine(description);

                if (headers.TryGetValue("X-PlantUML-Diagram-Error", out var error))
                {
                    errorDetails.AppendLine(error);
                }

                if (headers.TryGetValue("X-PlantUML-Diagram-Error-Line", out var errorLine))
                {
                    errorDetails.AppendLine($"Error-Line: {errorLine}");
                }

                return errorDetails.ToString().Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation failed.");
                return ex.Message; // Return the exception message if an error occurs
            }
        }

        private string EncodePlantuml(string uml)
        {
            byte[] compressedBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal, true))
                {
                    var inputBytes = Encoding.UTF8.GetBytes(uml);
                    deflateStream.Write(inputBytes, 0, inputBytes.Length);
                }
                compressedBytes = memoryStream.ToArray();
            }

            const string plantUmlChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
            var encoded = new StringBuilder();

            int current = 0;
            int bits = 0;

            foreach (var b in compressedBytes)
            {
                current = (current << 8) | b;
                bits += 8;

                while (bits >= 6)
                {
                    bits -= 6;
                    encoded.Append(plantUmlChars[(current >> bits) & 0x3F]);
                }
            }

            if (bits > 0)
            {
                encoded.Append(plantUmlChars[(current << (6 - bits)) & 0x3F]);
            }

            return encoded.ToString();
        }


        [McpServerTool, Description("Decodes an encoded PlantUML string and returns the original string.")]
        private string DecodePlantuml(string encoded)
        {
            const string plantUmlChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
            var charMap = plantUmlChars.Select((c, i) => new { c, i }).ToDictionary(x => x.c, x => x.i);

            var decodedBytes = new List<byte>();
            int current = 0;
            int bits = 0;

            foreach (var c in encoded)
            {
                if (!charMap.TryGetValue(c, out var value))
                {
                    throw new ArgumentException("Invalid character in encoded string.");
                }

                current = (current << 6) | value;
                bits += 6;

                if (bits >= 8)
                {
                    bits -= 8;
                    decodedBytes.Add((byte)((current >> bits) & 0xFF));
                }
            }

            using (var memoryStream = new MemoryStream(decodedBytes.ToArray()))
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(deflateStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}