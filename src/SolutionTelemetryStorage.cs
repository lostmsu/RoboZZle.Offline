namespace RoboZZle.Offline;

using System.Globalization;
using System.Threading.Tasks;

using LostTech.Checkpoint;

using PCLStorage;

/// <summary>
/// This class stores all session logs in a single folder.
/// Each session is stored in its own file, named after its start time in seconds.
/// </summary>
sealed class SolutionTelemetryStorage: ISolutionTelemetry {
    internal SolutionTelemetryStorage(IFolder folder) {
        this.Folder = folder;
    }

    internal readonly IFolder Folder;
    readonly AsyncChainService asyncChain = new();

    async Task Update(SessionLog sessionLog) {
        if (sessionLog == null)
            throw new ArgumentNullException(nameof(sessionLog));
        string fileName = GetFileName(sessionLog);
        var file = await this.Folder
                             .CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting)
                             .ConfigureAwait(false);
        DebugEx.WriteLine($"updating session log with {sessionLog.Entries.Count} entries");
        await file.WriteJson(sessionLog).ConfigureAwait(false);
    }

    public void QueueUpdate(SessionLog sessionLog) {
        if (sessionLog == null)
            throw new ArgumentNullException(nameof(sessionLog));

        sessionLog = sessionLog.Copy();
        this.asyncChain.Chain(() => this.Update(sessionLog));
    }

    static string GetFileName(SessionLog sessionLog) =>
        string.Format(CultureInfo.InvariantCulture,
                      "{0}s.json", sessionLog.StartTime.Ticks / TimeSpan.TicksPerSecond);

    public Task DisposeAsync() => this.asyncChain.DisposeAsync();
}