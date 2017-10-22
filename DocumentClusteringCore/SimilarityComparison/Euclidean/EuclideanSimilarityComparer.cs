using System;
using System.Linq;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.SimilarityComparison.Euclidean {
  public class EuclideanSimilarityComparer : ISimilarityComparer {
    public double GetSimilarity(Document docA, Document docB) {
      if (docA == null) {
        throw new ArgumentNullException(nameof(docA));
      }

      if (docB == null) {
        throw new ArgumentNullException(nameof(docB));
      }

      var allTerms = docA.InverseSetFrequencyWeights.Select(weight => weight.Key)
        .Union(docB.InverseSetFrequencyWeights.Select(weight => weight.Key));

      var weightsSum = allTerms.Aggregate(0d, (accumulator, term) => {
        var aWeight = docA.InverseSetFrequencyWeights.FirstOrDefault(weight => weight.Key == term).Value;
        var bWeight = docB.InverseSetFrequencyWeights.FirstOrDefault(weight => weight.Key == term).Value;

        var difference = Math.Pow(Math.Abs(aWeight - bWeight), 2);
        return accumulator + difference;
      });

      var distance = Math.Sqrt(weightsSum);
      return distance;
    }
  }
}
