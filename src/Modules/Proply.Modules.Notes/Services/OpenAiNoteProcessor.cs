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
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant that processes raw voice notes into structured JSON. You must return ONLY valid JSON matching the following structure: { \"summary\": \"string\", \"category\": \"Personal|Work|Ideas|Reminders\", \"tags\": [\"string\"], \"actionItems\": [\"string\"] }."),
            new UserChatMessage(rawContent)
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

        string jsonResponse = completion.Content[0].Text;
        
        // Basic cleanup if the model includes markdown code blocks
        if (jsonResponse.StartsWith("```json"))
        {
            jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();
        }

        // Deserialize using System.Text.Json (or Newtonsoft if preferred, but local logic here is fine with STJ or Newtonsoft)
        // Since we are inside the module and not persisting to Cosmos here, either is fine. 
        // Let's use System.Text.Json for simplicity as it is built-in, 
        // unless we want to be consistent with the rest of the app using Newtonsoft.
        // Let's use Newtonsoft to match the Domain definition attributes.
        return Newtonsoft.Json.JsonConvert.DeserializeObject<StructuredNote>(jsonResponse);
    }
}
