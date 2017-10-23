using System.Threading.Tasks;

namespace DocumentClusteringCore.Orchestration {
  public interface IWorkOrchestrator {
    Task ExecuteWorkAsync();
  }
}
