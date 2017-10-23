using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Normalization.Default {
  public class DefaultWeightNormalizer : IWeightNormalizer {
    private Dictionary<string, HashSet<string>> termDocumentAppearances;

    private readonly IMessageHub messageHub;

    public DefaultWeightNormalizer(IMessageHub messageHub) {
      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageHub.DocumentGenerated.Subscribe(OnDocumentGenerated);

      termDocumentAppearances = new Dictionary<string, HashSet<string>>();
      generatedDocuments = new List<Document>();
    }

    private List<Document> generatedDocuments;

    private void OnDocumentGenerated(Document generatedDocument) {
      if (generatedDocument == null) {
        throw new ArgumentNullException(nameof(generatedDocument));
      }
      
      foreach (var term in generatedDocument.TermCounts) {
        if (!termDocumentAppearances.ContainsKey(term.Key)) {
          termDocumentAppearances[term.Key] = new HashSet<string>();
        }

        termDocumentAppearances[term.Key].Add(generatedDocument.Name);
      }

      generatedDocuments.Add(generatedDocument);
    }

    public void NormalizeDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      var vectorNorm = Math.Sqrt(document.TermCounts.Select(kvp => Math.Sqrt(kvp.Value)).Sum());
      var countsDictionary = document.TermCounts
        .Select(kvp => kvp.Value / vectorNorm)
        .Select((x, index) => (key: document.TermCounts.ElementAt(index).Key, value: x))
        .ToDictionary(x => x.key, x => x.value);

      document.NormalizedTermWeights = new ReadOnlyDictionary<string, double>(countsDictionary);

      var inverseWeightsDictionary = new Dictionary<string, double>();
      foreach (var term in document.TermCounts) {
        var inverseTermWeight = term.Value * Math.Log(generatedDocuments.Count / termDocumentAppearances[term.Key].Count());
        inverseWeightsDictionary[term.Key] = inverseTermWeight;
      }

      document.InverseSetFrequencyWeights = new ReadOnlyDictionary<string, double>(inverseWeightsDictionary);
    }
  }
}
