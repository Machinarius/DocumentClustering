using System;
using System.Collections.Generic;

namespace DocumentClusteringCore.Orchestration.Models {
  public class ConfigureNormalizationAssignment : WorkAssignment {
    public int DocumentCount { get; }
    public IDictionary<string, int> TermDocumentAppearances { get; }

    public ConfigureNormalizationAssignment(int nodeId, int documentCount, IDictionary<string, int> termDocumentAppearances) : base(nodeId) {
      DocumentCount = documentCount;
      TermDocumentAppearances = termDocumentAppearances ?? throw new ArgumentNullException(nameof(termDocumentAppearances));
    }
  }
}
