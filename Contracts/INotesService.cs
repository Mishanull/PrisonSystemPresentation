using Entities;

namespace Contracts;

public interface INotesService
{
    public Task AddNoteAsync(long prisonerId, string text);
    public Task RemoveNoteAsync(long noteId);
    public Task UpdateNoteAsync(Note note);
}