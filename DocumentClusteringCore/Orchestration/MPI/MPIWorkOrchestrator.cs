using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Orchestration.Default;
using MPI;

namespace DocumentClusteringCore.Orchestration.MPI {
  public class MPIWorkOrchestrator : DefaultWorkOrchestrator {
    private readonly Intracommunicator communicator;

    private readonly IWorkerNodesLifecycleManager nodesManager;

    public MPIWorkOrchestrator(Options options, IMessageHub messageHub, IMessageSink messageSink, 
                           IWorkerNodesLifecycleManager nodesManager) 
      : base(options, messageHub, messageSink, nodesManager) {
      communicator = Communicator.world;

      this.nodesManager = nodesManager;
    }

    public override async Task ExecuteWorkAsync() {
      if (communicator.Rank == 0) {
        await base.ExecuteWorkAsync();

        return;
      }

      await nodesManager.CreateWorkerNodesAsync();
    }
  }
}
