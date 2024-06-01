namespace RoboZZle.Offline;

using System.Threading.Tasks;

/// <summary>
/// Represents in-progress solution telemetry
/// </summary>
public interface ISolutionTelemetry {
    /// <summary>
    /// Queues update for the specified solution editing log
    /// </summary>
    void QueueUpdate(SessionLog sessionLog);

    /// <summary>
    /// Asynchronously disposes this instance
    /// </summary>
    Task DisposeAsync();
}