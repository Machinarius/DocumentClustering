using System;

namespace DocumentClusteringCore.Orchestration.Models {
  internal class TokenizationAssignment : WorkAssignment {
    public string Filepath { get; }

    public TokenizationAssignment(int nodeId, string filename) : base(nodeId) {
      if (string.IsNullOrEmpty(filename)) {
        throw new ArgumentNullException(nameof(filename));
      }

      Filepath = filename;
    }
  }
}
