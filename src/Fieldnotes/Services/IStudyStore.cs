using Fieldnotes.Models;

namespace Fieldnotes.Services;

public interface IStudyStore
{
    Task<StudyDocument> LoadAsync();
    Task<bool> TrySaveAsync(StudyDocument document, long expectedRevision);
}