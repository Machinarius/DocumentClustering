using System;
using DryIoc;

namespace DocumentClusteringCore.Normalization.Default {
  public static class DefaultNormalizationContainerExtensions {
    public static void UseDefaultWeightNormalization(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<IWeightNormalizer, DefaultWeightNormalizer>(reuse: Reuse.Singleton);
    }
  }
}
