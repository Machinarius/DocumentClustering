using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Normalization.Default {
  public class DefaultWeightNormalizer : IWeightNormalizer {
    private int amountOfDocuments;
    private IDictionary<string, int> termDocumentAppearances;

    public void NormalizeDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      if (termDocumentAppearances == null) {
        throw new InvalidOperationException("You must call Configure first");
      }

      var vectorNorm = Math.Sqrt(document.TermCounts.Select(kvp => Math.Sqrt(kvp.Value)).Sum());
      var countsDictionary = document.TermCounts
        .Select(kvp => kvp.Value / vectorNorm)
        .Select((x, index) => (key: document.TermCounts.ElementAt(index).Key, value: x))
        .ToDictionary(x => x.key, x => x.value);

      document.NormalizedTermWeights = new ReadOnlyDictionary<string, double>(countsDictionary);

      var inverseWeightsDictionary = new Dictionary<string, double>();
      foreach (var term in document.TermCounts) {
        var inverseTermWeight = term.Value * Math.Log(amountOfDocuments / termDocumentAppearances[term.Key]);
        inverseWeightsDictionary[term.Key] = inverseTermWeight;
      }

      document.InverseSetFrequencyWeights = new ReadOnlyDictionary<string, double>(inverseWeightsDictionary);
    }

    public void Configure(int amountOfDocuments, IDictionary<string, int> termDocumentAppearances) {
      this.termDocumentAppearances = termDocumentAppearances ?? throw new ArgumentNullException(nameof(termDocumentAppearances));
      this.amountOfDocuments = amountOfDocuments;
    }
  }
}
