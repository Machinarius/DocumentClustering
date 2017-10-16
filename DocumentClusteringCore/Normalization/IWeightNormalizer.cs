using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Normalization {
  public interface IWeightNormalizer {
    void NormalizeDocument(Document document);
  }
}
