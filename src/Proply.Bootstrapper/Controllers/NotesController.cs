using Microsoft.AspNetCore.Mvc;
using Proply.Modules.Notes.Domain;
using Proply.Modules.Notes.Services;
using Proply.Shared.Kernel.Data;

namespace Proply.Bootstrapper.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteProcessor _noteProcessor;
    private readonly IRepository<VoiceNote> _noteRepository;

    public NotesController(INoteProcessor noteProcessor, IRepository<VoiceNote> noteRepository)
    {
        _noteProcessor = noteProcessor;
        _noteRepository = noteRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content cannot be empty.");
        }

        // 1. Process with AI to get structure
        var structuredNote = await _noteProcessor.ProcessNoteAsync(request.Content);

        // 2. Create Domain Entity
        var note = new VoiceNote
        {
            RawContent = request.Content,
            StructuredNote = structuredNote
        };

        // 3. Persist to Cosmos DB
        await _noteRepository.AddAsync(note);

        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNote(string id)
    {
        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }
}

public class CreateNoteRequest
{
    public string Content { get; set; } = string.Empty;
}
