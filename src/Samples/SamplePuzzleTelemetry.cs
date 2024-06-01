namespace RoboZZle.Offline;

using System.Threading.Tasks;

/// <summary>
/// Represents sample <see cref="IPuzzleTelemetry"/>
/// </summary>
public sealed class SamplePuzzleTelemetry: IPuzzleTelemetry {
    /// <summary>
    /// Gets sample solution-in-progress
    /// </summary>
    public ISolutionTelemetry SolutionInProgress { get; } = new SampleSolutionTelemetry();

    /// <summary>
    /// Does nothing
    /// </summary>
    public Task Victory(string currentProgram) => Task.FromResult(currentProgram);
}