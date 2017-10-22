using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.SimilarityComparison {
  public interface ISimilarityComparer {
    double GetSimilarity(Document docA, Document docB);
  }
}
