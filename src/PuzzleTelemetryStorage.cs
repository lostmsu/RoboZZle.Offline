namespace RoboZZle.Offline;

using System.Globalization;
using System.Threading.Tasks;

using PCLStorage;

using RoboZZle.Offline.Internal;

/// <summary>
/// This class stores puzzle telemetry in a single folder.
/// Each solution gets its own folder.
/// </summary>
sealed class PuzzleTelemetryStorage: IPuzzleTelemetry {
    const string STATE_FILE_NAME = "state.json";
    const string SOLVED_FILE_NAME = "solved.json";

    readonly IFolder folder;
    IFile? stateFile;
    PuzzleTelemetryState State { get; set; }
    SolutionTelemetryStorage CurrentSolution { get; set; }

    PuzzleTelemetryStorage(IFolder folder) {
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
    }

    /// <summary>
    /// Completes current solution, and starts a new one.
    /// </summary>
    public async Task Victory(string currentProgram) {
        IFile solvedFile =
            await this.CurrentSolution.Folder.CreateFileAsync(
                          SOLVED_FILE_NAME, CreationCollisionOption.FailIfExists)
                      .ConfigureAwait(false);

        await solvedFile.WriteJson(new CompletedSolution {
            StartingProgram = this.State.CurrentSolutionStartingProgram,
        }).ConfigureAwait(false);
        DebugEx.WriteLine(
            $"new solution {this.State.CurrentSolution} with starting program: {this.State.CurrentSolutionStartingProgram}");

        this.State = await this.ReinitializeState(currentProgram,
                                                  CreationCollisionOption.ReplaceExisting,
                                                  solution: this.State.CurrentSolution + 1)
                               .ConfigureAwait(false);

        this.CurrentSolution = await this.GetOrInitCurrentSolution().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets current solution in progress.
    /// </summary>
    public ISolutionTelemetry SolutionInProgress => this.CurrentSolution;

    async Task LoadState(string currentProgram) {
        this.stateFile = await this.folder.GetFileOrNull(STATE_FILE_NAME).ConfigureAwait(false);

        this.State = this.stateFile != null
            ? await this.stateFile.ReadJson<PuzzleTelemetryState>().ConfigureAwait(false)
            : await this.ReinitializeState(currentProgram).ConfigureAwait(false);

        this.CurrentSolution = await this.GetOrInitCurrentSolution().ConfigureAwait(false);
    }

    async Task<SolutionTelemetryStorage> GetOrInitCurrentSolution() {
        string currentSolutionFolderName =
            this.State.CurrentSolution.ToString(CultureInfo.InvariantCulture);
        var currentSolutionFolder = await this.folder
                                              .CreateFolderAsync(
                                                  currentSolutionFolderName,
                                                  CreationCollisionOption.OpenIfExists)
                                              .ConfigureAwait(false);
        return new SolutionTelemetryStorage(currentSolutionFolder);
    }

    async Task<PuzzleTelemetryState> ReinitializeState(string currentProgram,
                                                       CreationCollisionOption collisionOption =
                                                           CreationCollisionOption.FailIfExists,
                                                       int solution = 0) {
        DebugEx.WriteLine($"starting solution {solution}");
        this.stateFile = await this.folder.CreateFileAsync(STATE_FILE_NAME, collisionOption)
                                   .ConfigureAwait(false);
        var state = new PuzzleTelemetryState {
            CurrentSolution = solution,
            CurrentSolutionStartingProgram = currentProgram,
        };
        await this.stateFile.WriteJson(state).ConfigureAwait(false);
        return state;
    }

    public static async Task<PuzzleTelemetryStorage> Create(IFolder folder, string currentProgram) {
        var storage = new PuzzleTelemetryStorage(folder);
        await storage.LoadState(currentProgram).ConfigureAwait(false);
        return storage;
    }
}