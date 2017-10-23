using System;
using DryIoc;

namespace DocumentClusteringCore.Stemming.Default {
  public static class DefaultStemmingContainerExtensions {
    public static void UseCachedPorterStemming(this Container diContainer) {
      if (diContainer == null) {
        throw new ArgumentNullException(nameof(diContainer));
      }

      // TODO: Register this in a more clean way that allows the DI Container to properly build the stemmers
      var stemmer = new CachedWordStemmer(new PorterWordStemmer());
      diContainer.RegisterInstance<IWordStemmer>(stemmer);
    }
  }
}
