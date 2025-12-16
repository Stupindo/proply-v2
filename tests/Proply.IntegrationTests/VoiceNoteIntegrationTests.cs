using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Proply.Modules.Notes.Domain;
using Proply.Shared.Kernel.Data;
using Xunit;
using Xunit.Abstractions;

namespace Proply.IntegrationTests;

public class VoiceNoteIntegrationTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosRepository<VoiceNote> _repository;

    public VoiceNoteIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var endpoint = Configuration["CosmosDb:AccountEndpoint"];
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCredential = true,
            ExcludeVisualStudioCodeCredential = true
        });

        _cosmosClient = new CosmosClient(endpoint, credential);
        _repository = new CosmosRepository<VoiceNote>(_cosmosClient, Configuration["CosmosDb:DatabaseName"], Configuration["CosmosDb:ContainerName"]);
    }

    [Fact]
    public async Task Should_Create_And_Retrieve_VoiceNote()
    {
        // Arrange
        var note = new VoiceNote
        {
            Id = Guid.NewGuid().ToString(),
            RawContent = "Integration test note",
            StructuredNote = new StructuredNote
            {
                Summary = "Test Summary",
                Category = "Test Category",
                ActionItems = new List<string> { "Test Item 1" },
                Tags = new List<string> { "Test" }
            }
        };

        try
        {
            // Act
            _output.WriteLine($"Creating note with ID: {note.Id}");
            await _repository.AddAsync(note);

            _output.WriteLine("Retrieving note...");
            var retrievedNote = await _repository.GetByIdAsync(note.Id);

            // Assert
            Assert.NotNull(retrievedNote);
            Assert.Equal(note.Id, retrievedNote.Id);
            Assert.Equal(note.RawContent, retrievedNote.RawContent);
            Assert.Equal(note.StructuredNote.Summary, retrievedNote.StructuredNote.Summary);
        }
        finally
        {
            // Cleanup
            _output.WriteLine("Cleaning up...");
            await _repository.DeleteAsync(note.Id);
        }
    }

    [Fact]
    public async Task Should_Update_VoiceNote()
    {
        // Arrange
        var note = new VoiceNote
        {
            Id = Guid.NewGuid().ToString(),
            RawContent = "Original Content",
            StructuredNote = new StructuredNote { Summary = "Original Summary" }
        };

        await _repository.AddAsync(note);

        try
        {
            // Act
            note.RawContent = "Updated Content";
            await _repository.UpdateAsync(note);

            var retrievedNote = await _repository.GetByIdAsync(note.Id);

            // Assert
            Assert.NotNull(retrievedNote);
            Assert.Equal("Updated Content", retrievedNote.RawContent);
        }
        finally
        {
            // Cleanup
            await _repository.DeleteAsync(note.Id);
        }
    }

    [Fact]
    public async Task Should_Delete_VoiceNote()
    {
        // Arrange
        var note = new VoiceNote
        {
            Id = Guid.NewGuid().ToString(),
            RawContent = "To be deleted"
        };

        await _repository.AddAsync(note);

        // Act
        await _repository.DeleteAsync(note.Id);

        var retrievedNote = await _repository.GetByIdAsync(note.Id);

        // Assert
        Assert.Null(retrievedNote);
    }
}
