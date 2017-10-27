using System.Collections.Generic;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Normalization {
  public interface IWeightNormalizer {
    void Configure(int amountOfDocuments, IDictionary<string, int> termDocumentAppearances);
    void NormalizeDocument(Document document);
  }
}
