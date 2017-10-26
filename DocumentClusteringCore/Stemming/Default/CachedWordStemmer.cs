using System;
using System.Collections.Concurrent;

namespace DocumentClusteringCore.Stemming.Default {
  public class CachedWordStemmer : IWordStemmer {
    private readonly IWordStemmer backend;
    private readonly ConcurrentDictionary<string, string> cache;

    public CachedWordStemmer(IWordStemmer backend) {
      this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
      if (backend is CachedWordStemmer) {
        throw new ArgumentException("Nesting cached word stemmers is not allowed", nameof(backend));
      }

      cache = new ConcurrentDictionary<string, string>();
    }

    public string StemString(string target) {
      if (string.IsNullOrEmpty(target)) {
        throw new ArgumentException(nameof(target));
      }

      target = target.ToLower();
      if (cache.ContainsKey(target)) {
        return cache[target];
      }

      var stem = backend.StemString(target);
      cache[target] = stem;

      return stem;
    }
  }
}
