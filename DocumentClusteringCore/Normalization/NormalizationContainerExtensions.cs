using DryIoc;

namespace DocumentClusteringCore.Normalization {
  public static class NormalizationContainerExtensions {
    public static void UseDocumentNormalization(this Container diContainer) {
      if (diContainer == null) {
        throw new System.ArgumentNullException(nameof(diContainer));
      }


    }
  }
}
