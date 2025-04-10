using System.ComponentModel;
using ModelContextProtocol.Server;
using System.IO.Compression;
using System.Text;

namespace PlantUmlTools
{
    [McpServerToolType]
    public class PlantumlTool
    {
        private Uri PlantUmlBaseUrl { get; }
        public PlantumlTool(IConfiguration configuration, ILogger<PlantumlTool> logger)
        {
            var baseUrl = configuration["PlantUmlBaseUrl"];
            PlantUmlBaseUrl = !string.IsNullOrEmpty(baseUrl) ? new Uri(baseUrl) : new Uri("http://myplantuml:8080/");
            logger.LogInformation($"PlantUML Base URL: {PlantUmlBaseUrl}");
        }

#if DEBUG
        [McpServerTool, Description("connection test. echo message.")]
        public string Echo(string message)
        {
            return $"Hello {message}";
        }
#endif

#if DEBUG
        [McpServerTool, Description("Generates a PNG image from the provided PlantUML message and returns it as a Base64 string.")]
#endif
        public async Task<string> CreatePng(string message)
        {
            var encodedMessage = EncodePlantUml(message);
            Uri url = new (PlantUmlBaseUrl, $"png/{encodedMessage}");

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var imageBytes =
             await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(imageBytes);
        }

        [McpServerTool, Description("Validates the provided PlantUML message. If valid, returns 'Ok'. If invalid, returns detailed error information including error description, error line, and other metadata.")]
        public async Task<string> ValidatePlantUml(string message)
        {
            try
            {
                var encodedMessage = EncodePlantUml(message);
                Uri url = new (PlantUmlBaseUrl, $"txt/{encodedMessage}");
                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

                var errorDescription = headers.ContainsKey("X-PlantUML-Diagram-Description") && headers["X-PlantUML-Diagram-Description"].Contains("Error");
                if (errorDescription == false)
                {
                    return "Ok";
                }

                var errorDetails = new StringBuilder();
                errorDetails.AppendLine($"FAIL");
                errorDetails.AppendLine($"{headers["X-PlantUML-Diagram-Description"]}");

                if (headers.ContainsKey("X-PlantUML-Diagram-Error"))
                {
                    errorDetails.AppendLine($"{headers["X-PlantUML-Diagram-Error"]}");
                }

                if (headers.ContainsKey("X-PlantUML-Diagram-Error-Line"))
                {
                    errorDetails.AppendLine($"Error-Line: {headers["X-PlantUML-Diagram-Error-Line"]}");
                }

                return errorDetails.ToString().Trim();
            }
            catch (Exception ex)
            {
                return ex.Message; // Return the exception message if an error occurs
            }
        }

        private string EncodePlantUml(string uml)
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
    }
}
