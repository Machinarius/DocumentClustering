using System;
using System.Collections.ObjectModel;

namespace DocumentClusteringCore.Models {
  public sealed class Document {
    public string Name { get; }

    public ReadOnlyDictionary<string, int> TermCounts { get; }

    public ReadOnlyDictionary<string, double> NormalizedTermWeights { get; internal set; }

    public ReadOnlyDictionary<string, double> InverseSetFrequencyWeights { get; internal set; }

    public Document(string name, ReadOnlyDictionary<string, int> termCounts) {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      TermCounts = termCounts ?? throw new ArgumentNullException(nameof(termCounts));
    }
  }
}
