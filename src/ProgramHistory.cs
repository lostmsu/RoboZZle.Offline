namespace RoboZZle.Offline;

using System.Threading.Tasks;

using PCLStorage;

/// <summary>
/// Represents puzzle program version history
/// </summary>
public sealed class ProgramHistory: IProgramHistory {
    #region Private constructors and instance data

    public ProgramHistory(IFile historyFile, Puzzle puzzle) {
        this.historyFile = historyFile;
        this.puzzle = puzzle;
        this.CurrentVersion = -1;
    }

    readonly List<Program> versionHistory = [];
    readonly IFile? historyFile;
    readonly Puzzle puzzle;

    #endregion

    /// <summary>
    /// Index of current program's version
    /// </summary>
    public int CurrentVersion { get; private set; }
    /// <summary>
    /// Index of the latest program's version
    /// </summary>
    public int LatestVersion => this.versionHistory.Count - 1;

    /// <summary>
    /// Retrieves current program
    /// </summary>
    public Program CurrentProgram => this.versionHistory[this.CurrentVersion].Clone();

    /// <summary>
    /// Adds specified program version to version history.
    /// Any other version after the current one are discarded. (You can't Redo after Add)
    /// </summary>
    /// <param name="version">Program version to add to editing history</param>
    public void Add(Program version) {
        bool removeRange = this.CurrentVersion < this.LatestVersion;
        if (removeRange)
            this.versionHistory.RemoveRange(this.CurrentVersion + 1,
                                            this.LatestVersion - this.CurrentVersion);

        string? serialized = version.Encode(false);
        var copy = version.Clone();
        this.versionHistory.Add(copy);

        if (!removeRange)
            this.EnqueueDiskTask(file => file.AppendAllTextAsync(serialized + Environment.NewLine));
        else {
            var toSave = this.versionHistory.ToArray();
            this.EnqueueDiskTask(_ => this.Save(toSave));
        }

        this.CurrentVersion++;
    }

    /// <summary>
    /// Undoes last edit operation, returning previous program version.
    /// </summary>
    /// <returns>Previous program version.</returns>
    public Program Undo() {
        if (this.CurrentVersion <= 0)
            throw new InvalidOperationException("Can't revert further");

        this.CurrentVersion--;

        return this.CurrentProgram;
    }

    /// <summary>
    /// Redoes the last Undo operation in LIFO order.
    /// </summary>
    /// <returns>Restored program version.</returns>
    public Program Redo() {
        if (this.CurrentVersion >= this.LatestVersion)
            throw new InvalidOperationException("Can't redo: no undo was made recently");

        this.CurrentVersion++;

        return this.CurrentProgram;
    }

    /// <summary>
    /// Overwrites history file with current instance data
    /// </summary>
    public void Save() {
        var currentHistory = this.versionHistory.Take(this.CurrentVersion + 1).ToList();
        this.EnqueueDiskTask(_ => this.Save(currentHistory));
    }

    #region Storage

    async Task Save(IEnumerable<Program> versions) {
        string? history =
            await Task.Factory.StartNew(() => ToString(versions)).ConfigureAwait(false);
        await this.historyFile.WriteAllTextAsync(history).ConfigureAwait(false);
    }

    static string ToString(IEnumerable<Program> versions) {
        var serialized = versions.Select(v => v.Encode(false));
        return string.Join(Environment.NewLine, serialized);
    }

    Task taskQueue = Task.Run(() => { });

    void EnqueueDiskTask(Func<IFile, Task> action) {
        if (this.historyFile == null)
            return;

        this.taskQueue = this.taskQueue.ContinueWith(
            _ => action(this.historyFile).Wait(),
            TaskContinuationOptions.OnlyOnRanToCompletion
        );
    }

    /// <summary>
    /// Retrieves ProgramHistory for specified puzzle.
    /// </summary>
    /// <param name="store">Folder, that stores puzzle histories</param>
    /// <param name="puzzle">Puzzle to get ProgramHistory for</param>
    /// <returns>Async task, representing retreival</returns>
    public static async Task<ProgramHistory> Get(IFolder store, Puzzle puzzle) {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        var file = await store
                         .CreateFileAsync(puzzle.ID + ".txt", CreationCollisionOption.OpenIfExists)
                         .ConfigureAwait(false);
        var result = new ProgramHistory(file, puzzle);
        await result.Reload().ConfigureAwait(false);
        return result;
    }

    public Task Reload() {
        if (this.historyFile is null)
            throw new InvalidOperationException();

        return this.LoadFromFile(this.historyFile);
    }

    public async Task LoadFromFile(IFile file) {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        string[]? history = await file.ReadLinesAsync().ConfigureAwait(false);
        this.CurrentVersion = -1;
        this.versionHistory.Clear();
        foreach (string? line in history) {
            var version = Program.Parse(line, this.puzzle);
            this.versionHistory.Add(version);
            this.CurrentVersion++;
        }

        if (this.versionHistory.Count == 0) {
            var empty = new Program(this.puzzle);
            this.Add(empty);
        }
    }

    #endregion

    public IEnumerator<Program> GetEnumerator() => this.versionHistory.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => this.versionHistory.GetEnumerator();
}