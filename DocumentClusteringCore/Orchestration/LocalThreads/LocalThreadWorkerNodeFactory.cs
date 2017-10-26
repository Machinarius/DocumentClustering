using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Normalization;
using DocumentClusteringCore.SimilarityComparison;
using DocumentClusteringCore.Tokenization;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class LocalThreadWorkerNodeFactory : IWorkerNodeFactory {
    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;

    private readonly IDocumentTokenizer documentTokenizer;
    private readonly IWeightNormalizer weightNormalizer;
    private readonly ISimilarityComparer similarityComparer;
    private readonly Options options;

    public LocalThreadWorkerNodeFactory(IMessageHub messageHub, IMessageSink messageSink, 
                                        IDocumentTokenizer documentTokenizer, IWeightNormalizer weightNormalizer, 
                                        ISimilarityComparer similarityComparer, Options options) {
      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.documentTokenizer = documentTokenizer ?? throw new ArgumentNullException(nameof(documentTokenizer));
      this.weightNormalizer = weightNormalizer ?? throw new ArgumentNullException(nameof(weightNormalizer));
      this.similarityComparer = similarityComparer ?? throw new ArgumentNullException(nameof(similarityComparer));
      this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<IEnumerable<IWorkerNode>> CreateWorkerNodesAsync() {
      var nodeCount = options.NodeCount;
      if (nodeCount <= 0) {
        nodeCount = Environment.ProcessorCount;
      }

      var nodes = Enumerable.Range(0, nodeCount)
        .Select(CreateNode)
        .Cast<IWorkerNode>()
        .ToArray()
        .AsEnumerable();

      return Task.FromResult(nodes);
    }

    private LocalThreadWorkerNode CreateNode(int id) {
      return new LocalThreadWorkerNode(id, messageHub, messageSink, documentTokenizer, weightNormalizer, similarityComparer);
    }
  }
}
