using System;
using DryIoc;

namespace DocumentClusteringCore.Messaging.InProcess {
  public static class InProcessMessagingContainerExtensions {
    public static void UseInProcessMessaging(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      var messagingCenter = new InProcessMessagingCenter();
      diContainer.RegisterInstance<IMessageHub>(messagingCenter);
      diContainer.RegisterInstance<IMessageSink>(messagingCenter);
    }
  }
}
