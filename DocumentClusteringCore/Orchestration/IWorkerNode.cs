using System.Threading.Tasks;

namespace DocumentClusteringCore.Orchestration {
  public interface IWorkerNode {
    int Id { get; }
    Task StartAsync();
    Task StopAsync();
  }
}
