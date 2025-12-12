var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
