using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentClusteringCore.Orchestration {
  public interface IWorkerNodesLifecycleManager {
    Task<IEnumerable<IWorkerNode>> CreateWorkerNodesAsync();
    Task StopWorkerNodesAsync();
  }
}
