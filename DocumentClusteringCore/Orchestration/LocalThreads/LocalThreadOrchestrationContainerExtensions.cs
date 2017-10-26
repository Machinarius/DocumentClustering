using System;
using DryIoc;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public static class LocalThreadOrchestrationContainerExtensions {
    public static void UseLocalThreadWorkers(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<IWorkerNodeFactory, LocalThreadWorkerNodeFactory>();
      diContainer.Register<IWorkOrchestrator, DefaultWorkOrchestrator>();
    }
  }
}
