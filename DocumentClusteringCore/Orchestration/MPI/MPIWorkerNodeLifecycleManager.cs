using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Normalization;
using DocumentClusteringCore.Orchestration.LocalThreads;
using DocumentClusteringCore.Orchestration.Models;
using DocumentClusteringCore.SimilarityComparison;
using DocumentClusteringCore.Tokenization;
using MPI;

namespace DocumentClusteringCore.Orchestration.MPI {
  public class MPIWorkerNodeLifecycleManager : IWorkerNodesLifecycleManager {
    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;

    private readonly IDocumentTokenizer documentTokenizer;
    private readonly IWeightNormalizer weightNormalizer;
    private readonly ISimilarityComparer similarityComparer;
    private readonly Options options;

    private IEnumerable<IWorkerNode> nodes;

    private Intracommunicator communicator;

    public MPIWorkerNodeLifecycleManager() {
      communicator = Communicator.world;
    }

    public async Task<IEnumerable<IWorkerNode>> CreateWorkerNodesAsync() {
      if (communicator.Rank != 0) {
        var node = new LocalThreadWorkerNode(communicator.Rank, messageHub, messageSink, documentTokenizer,
                                             weightNormalizer, similarityComparer);

        await node.StartAsync();
        return new[] { node };
      }

      var nodeCount = communicator.Size;
      nodes = Enumerable.Range(0, nodeCount)
        .Select(id => new MPIRemoteWorkerNode(id));

      var tasks = nodes.Select(node => {
        var availabilityTCS = new TaskCompletionSource<bool>();
        
        messageHub.NodeAvailabilityChanges
          .Where(change => change.NodeId == node.Id && change.NodeAvailable)
          .FirstAsync()
          .Subscribe(change => availabilityTCS.SetResult(true));

        return availabilityTCS.Task;
      }).ToArray();

      await Task.WhenAll(tasks);
      return nodes;
    }

    public Task StopWorkerNodesAsync() {
      if (communicator.Rank != 0) {
        return Task.FromResult(0);
      }

      foreach (var node in nodes) {
        var assignment = new ShutdownAssignment(node.Id);
        messageSink.PostShutdownAssignment(assignment);
      }

      return Task.FromResult(0);
    }
  }
}
