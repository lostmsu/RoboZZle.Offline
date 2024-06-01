namespace RoboZZle.Offline;

using System.Globalization;
using System.Threading.Tasks;

using PCLStorage;

/// <summary>
/// Stores RoboZZle telemetry
/// </summary>
public sealed class TelemetryStorage {
    readonly IFolder folder;

    /// <summary>
    /// Creates new instance of <see cref="TelemetryStorage"/> in a specified folder.
    /// </summary>
    public TelemetryStorage(IFolder folder) {
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
    }

    /// <summary>
    /// Gets or starts new telemetry block for a puzzle
    /// </summary>
    public async Task<IPuzzleTelemetry> GetOrStartPuzzleTelemetry(
        int puzzleID, string currentProgram) {
        string folderName = puzzleID.ToString(CultureInfo.InvariantCulture);
        var puzzleFolder = await this.folder
                                     .CreateFolderAsync(folderName,
                                                        CreationCollisionOption.OpenIfExists)
                                     .ConfigureAwait(false);
        return await PuzzleTelemetryStorage.Create(puzzleFolder, currentProgram)
                                           .ConfigureAwait(false);
    }
}