using System;
using DryIoc;

namespace DocumentClusteringCore.Orchestration.MPI {
  public static class MPIOrchestrationContainerExtensions {
    public static void UseMPIOrchestration(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<IWorkOrchestrator, MPIWorkOrchestrator>();
      diContainer.Register<IWorkerNodesLifecycleManager, MPIWorkerNodeLifecycleManager>();
    }
  }
}
