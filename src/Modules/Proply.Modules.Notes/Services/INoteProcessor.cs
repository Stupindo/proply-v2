using Proply.Modules.Notes.Domain;

namespace Proply.Modules.Notes.Services;

public interface INoteProcessor
{
    Task<StructuredNote> ProcessNoteAsync(string rawContent);
}
