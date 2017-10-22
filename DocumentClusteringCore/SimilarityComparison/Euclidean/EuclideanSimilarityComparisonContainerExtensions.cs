using System;
using DryIoc;

namespace DocumentClusteringCore.SimilarityComparison.Euclidean {
  public static class EuclideanSimilarityComparisonContainerExtensions {
    public static void UseEuclideanSimilarityComparison(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<ISimilarityComparer, EuclideanSimilarityComparer>();
    }
  }
}
