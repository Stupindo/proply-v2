using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Proply.Modules.Notes.Domain;
using System.Text.Json;

namespace Proply.Modules.Notes.Services;

public class OpenAiNoteProcessor : INoteProcessor
{
    private readonly ChatClient _chatClient;

    public OpenAiNoteProcessor(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<StructuredNote> ProcessNoteAsync(string rawContent)
    {
        // Define the JSON schema for StructuredNote
        var jsonSchema = BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new
            {
                summary = new { type = "string" },
                category = new { type = "string", @enum = new[] { "Personal", "Work", "Ideas", "Reminders" } },
                tags = new { type = "array", items = new { type = "string" } },
                actionItems = new { type = "array", items = new { type = "string" } }
            },
            required = new[] { "summary", "category", "tags", "actionItems" },
            additionalProperties = false
        });

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "structured_note",
                jsonSchema: jsonSchema,
                jsonSchemaIsStrict: true)
        };

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant that processes raw voice notes."),
            new UserChatMessage(rawContent)
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);

        // The model is guaranteed to return valid JSON matching the schema
        string jsonResponse = completion.Content[0].Text;

        // Deserialize using System.Text.Json
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<StructuredNote>(jsonResponse, jsonOptions);
    }
}
