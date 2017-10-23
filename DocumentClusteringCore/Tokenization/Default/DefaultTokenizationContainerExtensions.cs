using System;
using DryIoc;

namespace DocumentClusteringCore.Tokenization.Default {
  public static class DefaultTokenizationContainerExtensions {
    public static void UseDefaultTokenization(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      diContainer.Register<IDocumentTokenizer, DefaultDocumentTokenizer>();
    }
  }
}
