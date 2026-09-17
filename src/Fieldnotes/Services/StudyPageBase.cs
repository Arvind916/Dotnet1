using Fieldnotes.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Fieldnotes.Services;

public abstract class StudyPageBase : ComponentBase
{
    [Inject] protected StudyService Study { get; set; } = null!;
    [Inject] protected ConceptCatalog Catalog { get; set; } = null!;
    [Inject] protected TimeProvider Clock { get; set; } = null!;
    [Inject] private ILogger<StudyPageBase> Logger { get; set; } = null!;
    protected StudySnapshot Snapshot { get; private set; } = StudySnapshot.Empty;
    protected bool IsBusy { get; private set; }
    protected bool IsReady { get; private set; }
    protected string? Error { get; private set; }
    protected DateTime Now => Clock.GetUtcNow().UtcDateTime;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Snapshot = await Study.GetSnapshotAsync();
            IsReady = true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load study progress");
            Error = "Browser storage is unavailable. Check this site's storage permissions and reload.";
        }
    }

    protected async Task<bool> SaveAsync(Func<Task> operation)
    {
        if (IsBusy)
        {
            return false;
        }

        IsBusy = true;
        Error = null;
        try
        {
            await operation();
            Snapshot = await Study.GetSnapshotAsync();
            IsReady = true;
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not save study progress");
            Error = "Your change could not be saved in this browser. Check available storage and try again.";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected Task<bool> BookmarkAsync(string conceptId) => SaveAsync(() => Study.ToggleBookmarkAsync(conceptId));
}