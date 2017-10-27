using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Orchestration.Default {
  public class DefaultWorkOrchestrator : IWorkOrchestrator {
    private readonly Options options;

    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;
    private readonly IWorkerNodesLifecycleManager nodesManager;
    private readonly List<IDisposable> subscriptions;

    private readonly Queue<string> filepathsToTokenize;

    private readonly List<Document> generatedDocuments;
    private readonly Queue<Document> documentsToNormalize;
    private readonly Dictionary<string, int> termDocumentAppearances;

    private readonly TaskCompletionSource<int> tokenizationDoneTCS;

    private IWorkerNode[] workerNodes;

    public DefaultWorkOrchestrator(Options options, IMessageHub messageHub, IMessageSink messageSink,
                                   IWorkerNodesLifecycleManager nodesManager) {
      this.options = options ?? throw new ArgumentNullException(nameof(options));
      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
      subscriptions = new List<IDisposable>();

      filepathsToTokenize = new Queue<string>(options.FilePaths);

      generatedDocuments = new List<Document>(filepathsToTokenize.Count());
      documentsToNormalize = new Queue<Document>(filepathsToTokenize.Count());
      termDocumentAppearances = new Dictionary<string, int>();

      tokenizationDoneTCS = new TaskCompletionSource<int>();
    }

    public virtual async Task ExecuteWorkAsync() {
      subscriptions.Add(messageHub.DocumentTokenized
        .ObserveOn(Scheduler.Immediate)
        //.SubscribeOn(SynchronizationContext.Current) // TODO: Find out why this doesn't work
        .Subscribe(OnDocumentGenerated));

      subscriptions.Add(messageHub.NodeAvailabilityChanges
        .ObserveOn(Scheduler.Default)
        //.SubscribeOn(SynchronizationContext.Current)
        .Subscribe(OnNodeAvailabilityChanged));

      await WorkLoopAsync();
    }

    private void OnDocumentGenerated(Document generatedDocument) {
      if (generatedDocument == null) {
        throw new ArgumentNullException(nameof(generatedDocument));
      }

      generatedDocuments.Add(generatedDocument);
      documentsToNormalize.Enqueue(generatedDocument);
      
      foreach (var termKvp in generatedDocument.TermCounts) {
        var actualCount = 0;
        if (termDocumentAppearances.ContainsKey(termKvp.Key)) {
          actualCount = termDocumentAppearances[termKvp.Key];
        }

        actualCount = actualCount + 1;
        termDocumentAppearances[termKvp.Key] = actualCount;
      }

      if (!filepathsToTokenize.Any()) {
        tokenizationDoneTCS.SetResult(0);
      }
    }
    
    private void OnNodeAvailabilityChanged(NodeAvailabilityChange availabilityChange) {
      if (availabilityChange == null) {
        throw new ArgumentNullException(nameof(availabilityChange));
      }

      Debug.WriteLine($"Availability change: node {availabilityChange.NodeId} - {availabilityChange.NodeAvailable}");
    }

    private async Task WorkLoopAsync() {
      var nodes = await nodesManager.CreateWorkerNodesAsync();
      workerNodes = nodes.ToArray();

      var nodesEnumerator = nodesStream().GetEnumerator();
      nodesEnumerator.MoveNext();

      var assignments = assignmentsStream();
      foreach (var assignmentFunction in assignments) {
        var assignedNode = nodesEnumerator.Current;
        var advanceNode = await assignmentFunction(assignedNode);
        if (advanceNode) {
          nodesEnumerator.MoveNext();
        }
      }

      await nodesManager.StopWorkerNodesAsync();
      
      IEnumerable<Func<int, Task<bool>>> assignmentsStream() {
        while (filepathsToTokenize.Any()) {
          var nextFilePath = filepathsToTokenize.Dequeue();

          yield return (nodeId) => {
            var assignment = new TokenizationAssignment(nodeId, nextFilePath);
            messageSink.PostTokenizationAssignment(assignment);

            return Task.FromResult(true);
          }; 
        }

        yield return (nodeId) => tokenizationDoneTCS.Task.ContinueWith(_ => false);

        foreach (var node in workerNodes) {
          yield return (nodeId) => {
            var assignment = new ConfigureNormalizationAssignment(nodeId, generatedDocuments.Count(), termDocumentAppearances);
            messageSink.PostConfigureNormalizationAssignment(assignment);

            return Task.FromResult(true);
          };
        }
        
        while (documentsToNormalize.Any()) {
          var nextDocument = documentsToNormalize.Dequeue();

          yield return (nodeId) => {
            var assignment = new NormalizationAssignment(nodeId, nextDocument);
            messageSink.PostNormalizationAssignment(assignment);

            return Task.FromResult(true);
          };
        }

        yield break;
      }

      IEnumerable<int> nodesStream() {
        var currentNodeId = 0;

        while (true) {
          currentNodeId++;
          if (currentNodeId > workerNodes.Length) {
            currentNodeId = 0;
          }

          yield return currentNodeId;
        }
      }
    }
  }
}
