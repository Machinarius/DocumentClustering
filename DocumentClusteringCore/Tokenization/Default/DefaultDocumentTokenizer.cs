using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.TermFiltering;

namespace DocumentClusteringCore.Tokenization.Default {
  public class DefaultDocumentTokenizer : IDocumentTokenizer {
    private readonly ITermSieve termSieve;

    public DefaultDocumentTokenizer(ITermSieve termSieve) {
      this.termSieve = termSieve ?? throw new ArgumentNullException(nameof(termSieve));
    }

    public Document TokenizeStream(Stream textStream, string name) {
      if (textStream == null) {
        throw new ArgumentNullException(nameof(textStream));
      }

      if (string.IsNullOrEmpty(name)) {
        throw new ArgumentException(nameof(name));
      }

      var terms = termSieve.GetTextTerms(textStream);
      var weights = terms.GroupBy(term => term)
        .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

      var roWeights = new ReadOnlyDictionary<string, int>(weights);
      return new Document(name, roWeights);
    }
  }
}
