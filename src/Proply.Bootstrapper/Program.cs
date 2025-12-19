
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure Swagger/OpenAPI
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

/*
// 1. Enable logging to Console
var listener = Azure.Core.Diagnostics.AzureEventSourceListener.CreateConsoleLogger(System.Diagnostics.Tracing.EventLevel.LogAlways);
*/

// Initialize the credential with options matching integration tests
var credential = new Azure.Identity.DefaultAzureCredential(new Azure.Identity.DefaultAzureCredentialOptions
{
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true
});

var keyVaultUri = builder.Configuration["KeyVaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
}

// Register Cosmos Client
builder.Services.AddSingleton<Microsoft.Azure.Cosmos.CosmosClient>(sp =>
{
    var cosmosSection = builder.Configuration.GetSection("CosmosDb");
    var endpoint = cosmosSection["AccountEndpoint"];
    if (string.IsNullOrEmpty(endpoint))
    {
        // Fallback or throw? For now let's minimal handling
        throw new InvalidOperationException("CosmosDb:AccountEndpoint must be configured."); 
    }
    
    // Use the shared credential
    return new Microsoft.Azure.Cosmos.CosmosClient(endpoint, credential);
});

// Configure Azure OpenAI
builder.Services.AddSingleton<OpenAI.Chat.ChatClient>(sp =>
{
    var openAiSection = builder.Configuration.GetSection("AzureOpenAi");
    var endpoint = openAiSection["Endpoint"];
    var deployment = openAiSection["DeploymentName"];

    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deployment))
    {
        throw new InvalidOperationException("AzureOpenAi:Endpoint and DeploymentName must be configured.");
    }

    // AzureOpenAIClient requires Azure.AI.OpenAI package
    var client = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), credential);
    return client.GetChatClient(deployment);
});

// Register Module Services
builder.Services.AddScoped<Proply.Modules.Notes.Services.INoteProcessor, Proply.Modules.Notes.Services.OpenAiNoteProcessor>();

// Register Repositories
builder.Services.AddScoped<Proply.Shared.Kernel.Data.IRepository<Proply.Modules.Notes.Domain.VoiceNote>>(sp =>
{
    var client = sp.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
    var cosmosSection = builder.Configuration.GetSection("CosmosDb");
    var databaseName = cosmosSection["DatabaseName"] ?? throw new InvalidOperationException("CosmosDb:DatabaseName is missing");
    var containerName = cosmosSection["ContainerName"] ?? throw new InvalidOperationException("CosmosDb:ContainerName is missing");
    return new Proply.Shared.Kernel.Data.CosmosRepository<Proply.Modules.Notes.Domain.VoiceNote>(client, databaseName, containerName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

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
