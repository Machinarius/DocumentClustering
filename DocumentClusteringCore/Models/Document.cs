using System;
using System.Collections.ObjectModel;

namespace DocumentClusteringCore.Models {
  public sealed class Document {
    public string Name { get; }
    public ReadOnlyDictionary<string, int> TermCounts { get; }

    public Document(string name, ReadOnlyDictionary<string, int> termWeights) {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      TermCounts = termWeights ?? throw new ArgumentNullException(nameof(termWeights));
    }
  }
}
