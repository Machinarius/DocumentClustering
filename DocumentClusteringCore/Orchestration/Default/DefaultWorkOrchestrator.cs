using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class DefaultWorkOrchestrator : IWorkOrchestrator {
    private readonly Options options;

    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;
    private readonly IWorkerNodeFactory nodeFactory;
    private readonly List<IDisposable> subscriptions;

    private readonly Queue<string> filepathsToTokenize;

    private readonly List<Document> generatedDocuments;
    private readonly Queue<Document> documentsToNormalize;

    private IWorkerNode[] workerNodes;
    
    private readonly TaskCompletionSource<int> workersReadyTCS;

    public DefaultWorkOrchestrator(Options options, IMessageHub messageHub, IMessageSink messageSink,
                                       IWorkerNodeFactory nodeFactory) {
      this.options = options ?? throw new ArgumentNullException(nameof(options));
      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
      subscriptions = new List<IDisposable>();

      workersReadyTCS = new TaskCompletionSource<int>();

      filepathsToTokenize = new Queue<string>(options.FilePaths);

      generatedDocuments = new List<Document>(filepathsToTokenize.Count());
      documentsToNormalize = new Queue<Document>(filepathsToTokenize.Count());
    }

    public async Task ExecuteWorkAsync() {
      subscriptions.Add(messageHub.DocumentGenerated
        //.SubscribeOn(SynchronizationContext.Current) // TODO: Find out why this doesn't work
        .Subscribe(OnDocumentGenerated));

      subscriptions.Add(messageHub.NodeAvailabilityChanges
        //.SubscribeOn(SynchronizationContext.Current)
        .Subscribe(OnNodeAvailabilityChanged));

      var nodes = await nodeFactory.CreateWorkerNodesAsync();
      workerNodes = nodes.ToArray();

      foreach (var node in workerNodes) {
        await node.StartAsync();
      }

      await WorkLoopAsync();
    }

    private void OnDocumentGenerated(Document generatedDocument) {
      if (generatedDocument == null) {
        throw new ArgumentNullException(nameof(generatedDocument));
      }

      generatedDocuments.Add(generatedDocument);
      documentsToNormalize.Enqueue(generatedDocument);
    }

    private int readyWorkersAmount;

    private void OnNodeAvailabilityChanged(NodeAvailabilityChange availabilityChange) {
      if (availabilityChange == null) {
        throw new ArgumentNullException(nameof(availabilityChange));
      }

      Debug.WriteLine($"Availability change: node {availabilityChange.NodeId} - {availabilityChange.NodeAvailable}");

      if (readyWorkersAmount < workerNodes.Length) {
        readyWorkersAmount++;
        if (readyWorkersAmount == workerNodes.Length) {
          workersReadyTCS.SetResult(0);
        }
      }
    }

    private async Task WorkLoopAsync() {
      await workersReadyTCS.Task;

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

      await StopWorkersAsync();
      
      IEnumerable<Func<int, Task<bool>>> assignmentsStream() {
        while (filepathsToTokenize.Any()) {
          var nextFilePath = filepathsToTokenize.Dequeue();

          yield return (nodeId) => {
            var assignment = new TokenizationAssignment(nodeId, nextFilePath);
            messageSink.PostTokenizationAssignment(assignment);

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

    private async Task StopWorkersAsync() {
      if (workerNodes == null) {
        return;
      }

      foreach (var node in workerNodes) {
        await node.StopAsync();
      }
      workerNodes = null;
    }
  }
}
