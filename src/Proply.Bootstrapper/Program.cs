
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

/*
// 1. Enable logging to Console
var listener = Azure.Core.Diagnostics.AzureEventSourceListener.CreateConsoleLogger(System.Diagnostics.Tracing.EventLevel.LogAlways);

// 2. Initialize the credential (this will now output logs)
var credential = new DefaultAzureCredential();
*/
var keyVaultUri = builder.Configuration["KeyVaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new Azure.Identity.DefaultAzureCredential());
}

// Register Cosmos Client
builder.Services.AddSingleton<Microsoft.Azure.Cosmos.CosmosClient>(sp =>
{
    var cosmosSection = builder.Configuration.GetSection("CosmosDb");
    var endpoint = cosmosSection["AccountEndpoint"];
    if (string.IsNullOrEmpty(endpoint))
    {
        // Fallback or throw? For now let's minimal handling
        return null; 
    }
    
    // Use DefaultAzureCredential which is already added for KeyVault.
    // CosmosClient supports TokenCredential.
    return new Microsoft.Azure.Cosmos.CosmosClient(endpoint, new Azure.Identity.DefaultAzureCredential());
});

// Configure Azure OpenAI
builder.Services.AddSingleton<OpenAI.Chat.ChatClient>(sp =>
{
    var openAiSection = builder.Configuration.GetSection("AzureOpenAi");
    var endpoint = openAiSection["Endpoint"];
    var deployment = openAiSection["DeploymentName"];

    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deployment))
    {
        return null;
    }

    // AzureOpenAIClient requires Azure.AI.OpenAI package
    var client = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), new Azure.Identity.DefaultAzureCredential());
    return client.GetChatClient(deployment);
});

// Register Module Services
builder.Services.AddScoped<Proply.Modules.Notes.Services.INoteProcessor, Proply.Modules.Notes.Services.OpenAiNoteProcessor>();

// Register Repositories
builder.Services.AddScoped<Proply.Shared.Kernel.Data.IRepository<Proply.Modules.Notes.Domain.VoiceNote>>(sp =>
{
    var client = sp.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
    return new Proply.Shared.Kernel.Data.CosmosRepository<Proply.Modules.Notes.Domain.VoiceNote>(client, "ProplyDb", "Notes");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapControllers(); // Enable controllers
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
