using System.Text.Json;
using Fieldnotes.Models;
using Microsoft.JSInterop;

namespace Fieldnotes.Services;

public sealed class BrowserStudyStore(IJSRuntime javascript) : IStudyStore
{
    public async Task<StudyDocument> LoadAsync()
    {
        var json = await javascript.InvokeAsync<string>("fieldnotesStorage.load");
        return JsonSerializer.Deserialize(json, StudyJsonContext.Default.StudyDocument) ?? new StudyDocument();
    }

    public async Task<bool> TrySaveAsync(StudyDocument document, long expectedRevision) =>
        await javascript.InvokeAsync<bool>("fieldnotesStorage.trySave",
            JsonSerializer.Serialize(document, StudyJsonContext.Default.StudyDocument), expectedRevision);
}