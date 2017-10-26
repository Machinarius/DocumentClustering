using System;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Orchestration.Models {
  public class NormalizationAssignment : WorkAssignment {
    public Document NormalizationSubject { get; }

    public NormalizationAssignment(int nodeId, Document normalizationSubject) : base(nodeId) {
      NormalizationSubject = normalizationSubject ?? throw new ArgumentNullException(nameof(normalizationSubject));
    }
  }
}
