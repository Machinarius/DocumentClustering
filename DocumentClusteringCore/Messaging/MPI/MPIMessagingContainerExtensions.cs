using System;
using DryIoc;

namespace DocumentClusteringCore.Messaging.MPI {
  public static class MPIMessagingContainerExtensions {
    public static void UseMPIMessaging(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<IMessageHub, MPIMessageHub>(reuse: Reuse.Singleton);
      diContainer.Register<IMessageSink, MPIMessageSink>(reuse: Reuse.Singleton);
    }
  }
}
