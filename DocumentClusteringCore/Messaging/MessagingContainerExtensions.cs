using DocumentClusteringCore.Messaging.Implementations;
using DryIoc;

namespace DocumentClusteringCore.Messaging {
  public static class MessagingContainerExtensions {
    public static void UseMessaging(this Container diContainer) {
      if (diContainer == null) {
        throw new System.ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<IMessageHub, DefaultMessagingCenter>(reuse: Reuse.Singleton);
      diContainer.Register<IMessageSink, DefaultMessagingCenter>(reuse: Reuse.Singleton);
    }
  }
}
