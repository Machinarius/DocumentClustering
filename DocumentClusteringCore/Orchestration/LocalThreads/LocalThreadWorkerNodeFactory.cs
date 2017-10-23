using System;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Normalization;
using DocumentClusteringCore.SimilarityComparison;
using DocumentClusteringCore.Tokenization;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class LocalThreadWorkerNodeFactory {
    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;

    private readonly IDocumentTokenizer documentTokenizer;
    private readonly IWeightNormalizer weightNormalizer;
    private readonly ISimilarityComparer similarityComparer;

    public LocalThreadWorkerNodeFactory(IMessageHub messageHub, IMessageSink messageSink, 
                                        IDocumentTokenizer documentTokenizer, IWeightNormalizer weightNormalizer, 
                                        ISimilarityComparer similarityComparer) {
      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.documentTokenizer = documentTokenizer ?? throw new ArgumentNullException(nameof(documentTokenizer));
      this.weightNormalizer = weightNormalizer ?? throw new ArgumentNullException(nameof(weightNormalizer));
      this.similarityComparer = similarityComparer ?? throw new ArgumentNullException(nameof(similarityComparer));
    }

    internal LocalThreadWorkerNode CreateNode(int id) {
      return new LocalThreadWorkerNode(id, messageHub, messageSink);
    }
  }
}
