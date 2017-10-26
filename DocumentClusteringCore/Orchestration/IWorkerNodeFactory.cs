using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentClusteringCore.Orchestration {
  public interface IWorkerNodeFactory {
    Task<IEnumerable<IWorkerNode>> CreateWorkerNodesAsync();
  }
}
