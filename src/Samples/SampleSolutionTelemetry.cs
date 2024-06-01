namespace RoboZZle.Offline;

using System.Threading.Tasks;

class SampleSolutionTelemetry: ISolutionTelemetry {
    public Task DisposeAsync() => Task.FromResult(42);
    public void QueueUpdate(SessionLog sessionLog) { }
    public Task Update(SessionLog sessionLog) => Task.FromResult(42);
}