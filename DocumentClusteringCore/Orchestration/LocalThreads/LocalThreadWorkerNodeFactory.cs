using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Normalization;
using DocumentClusteringCore.SimilarityComparison;
using DocumentClusteringCore.Tokenization;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class LocalThreadWorkerNodeFactory : IWorkerNodesLifecycleManager {
    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;

    private readonly IDocumentTokenizer documentTokenizer;
    private readonly IWeightNormalizer weightNormalizer;
    private readonly ISimilarityComparer similarityComparer;
    private readonly Options options;

    private IEnumerable<LocalThreadWorkerNode> nodes;

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

    public async Task<IEnumerable<IWorkerNode>> CreateWorkerNodesAsync() {
      if (nodes != null) {
        return nodes;
      }

      var nodeCount = options.NodeCount;
      if (nodeCount <= 0) {
        nodeCount = Environment.ProcessorCount;
      }

      nodes = Enumerable.Range(0, nodeCount)
        .Select(CreateNode)
        .ToArray();

      var readyTasks = nodes.Select(node => {
        var availabilityTCS = new TaskCompletionSource<bool>();
        messageHub.NodeAvailabilityChanges
          .Where(change => change.NodeId == node.Id && change.NodeAvailable)
          .FirstAsync()
          .Subscribe(change => availabilityTCS.SetResult(true));

        node.StartAsync();

        return availabilityTCS.Task;
      }).ToArray();

      await Task.WhenAll(readyTasks);

      return nodes.Cast<IWorkerNode>();
    }

    public async Task StopWorkerNodesAsync() {
      if (nodes == null) {
        return;
      }

      foreach (var node in nodes) {
        await node.StopAsync();
      }
    }

    private LocalThreadWorkerNode CreateNode(int id) {
      return new LocalThreadWorkerNode(id, messageHub, messageSink, documentTokenizer, weightNormalizer, similarityComparer);
    }
  }
}
