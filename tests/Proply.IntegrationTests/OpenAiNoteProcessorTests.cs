using Azure.Identity;
using Azure.Core;
using OpenAI.Chat;
using Proply.Modules.Notes.Services;

namespace Proply.IntegrationTests;

public class OpenAiNoteProcessorTests : TestBase
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public OpenAiNoteProcessorTests(Xunit.Abstractions.ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Should_Process_VoiceNote_Correctly()
    {
        try
        {
            // Arrange
            var endpoint = Configuration["AzureOpenAi:Endpoint"];
            var deployment = Configuration["AzureOpenAi:DeploymentName"];

            _output.WriteLine($"Endpoint: {endpoint}");
            _output.WriteLine($"Deployment: {deployment}");

            Assert.False(string.IsNullOrEmpty(endpoint), "AzureOpenAi:Endpoint is missing in configuration.");
            Assert.False(string.IsNullOrEmpty(deployment), "AzureOpenAi:DeploymentName is missing in configuration.");

            // Use AzureCliCredential to enforce usage of the logged-in CLI user
            var credential = new AzureCliCredential();
            
            // Verify we can get a token first
            _output.WriteLine("Attempting to acquire token for https://cognitiveservices.azure.com/.default ...");
            var tokenContext = new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" });
            var token = await credential.GetTokenAsync(tokenContext);
            _output.WriteLine($"Token acquired successfully. Expires: {token.ExpiresOn}");

            var azureClient = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), credential);
            ChatClient chatClient = azureClient.GetChatClient(deployment);

            var processor = new OpenAiNoteProcessor(chatClient);
        
            var rawNote = "Remind me to call John tomorrow about the project update.";

            // Act
            _output.WriteLine("Sending request to Azure OpenAI...");
            var result = await processor.ProcessNoteAsync(rawNote);
            _output.WriteLine("Request successful.");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Reminders", result.Category);
            Assert.Contains(result.ActionItems, item => item.Contains("call John", StringComparison.OrdinalIgnoreCase));
            Assert.False(string.IsNullOrWhiteSpace(result.Summary));
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test Failed with Exception: {ex.GetType().Name}");
            _output.WriteLine($"Message: {ex.Message}");
            throw; // Re-throw to fail the test but allow us to see the log
        }
    }
}
