using System;
using PorterStemmer;

namespace DocumentClusteringCore.Stemming.Implementations {
  public class PorterWordStemmer : IWordStemmer {
    public string StemString(string target) {
      if (string.IsNullOrEmpty(target)) {
        throw new ArgumentException(nameof(target));
      }

      target = target.ToLower();

      var stemmer = new Stemmer();
      foreach (var character in target) {
        stemmer.Add(character);
      }

      stemmer.Stem();
      if (stemmer.GetResultLength() != target.Length) {
        // The stemming process removed nothing
        return target;
      }

      var stem = stemmer.ToString();
      return stem;
    }
  }
}
