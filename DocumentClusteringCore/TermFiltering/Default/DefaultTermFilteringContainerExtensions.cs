using System;
using DryIoc;

namespace DocumentClusteringCore.TermFiltering.Default {
  public static class DefaultTermFilteringContainerExtensions {
    public static void UseDefaultTermFiltering(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<ITermSieve, DefaultTermSieve>(reuse: Reuse.Singleton);
    }
  }
}
