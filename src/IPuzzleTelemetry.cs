namespace RoboZZle.Offline;

using System.Threading.Tasks;

/// <summary>
/// Represents puzzle-related telemetry
/// </summary>
public interface IPuzzleTelemetry {
    /// <summary>
    /// Gets current solution-in-progress
    /// </summary>
    ISolutionTelemetry SolutionInProgress { get; }

    /// <summary>
    /// Marks current solution-in-progress as completed.
    /// </summary>
    Task Victory(string currentProgram);
}