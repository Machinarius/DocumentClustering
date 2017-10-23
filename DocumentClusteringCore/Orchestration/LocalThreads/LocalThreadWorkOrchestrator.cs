using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class LocalThreadWorkOrchestrator : IWorkOrchestrator {
    private readonly Options options;

    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;
    private readonly LocalThreadWorkerNodeFactory nodeFactory;
    private List<IDisposable> subscriptions;

    private Queue<string> filepathsToTokenize;

    private ConcurrentBag<Document> generatedDocuments;
    private ConcurrentQueue<Document> documentsToNormalize;

    private LocalThreadWorkerNode[] workerNodes;

    private TaskCompletionSource<int> workTCS;

    public LocalThreadWorkOrchestrator(Options options, IMessageHub messageHub, IMessageSink messageSink,
                                       LocalThreadWorkerNodeFactory nodeFactory) {
      this.options = options ?? throw new ArgumentNullException(nameof(options));
      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
      subscriptions = new List<IDisposable>();

      workTCS = new TaskCompletionSource<int>();

      filepathsToTokenize = new Queue<string>(options.Filenames);

      generatedDocuments = new ConcurrentBag<Document>();
      documentsToNormalize = new ConcurrentQueue<Document>();
    }

    public Task ExecuteWorkAsync() {
      subscriptions.Add(messageHub.DocumentGenerated.Subscribe(OnDocumentGenerated));
      subscriptions.Add(messageHub.NodeAvailabilityChanges.Subscribe(OnNodeAvailabilityChanged));

      var nodeCount = options.NodeCount;
      if (nodeCount <= 0) {
        nodeCount = Environment.ProcessorCount;
      }

      workerNodes = Enumerable.Range(0, nodeCount)
        .Select(id => nodeFactory.CreateNode(id)).ToArray();

      return workTCS.Task;
    }

    private void OnDocumentGenerated(Document generatedDocument) {
      if (generatedDocument == null) {
        throw new ArgumentNullException(nameof(generatedDocument));
      }


    }

    private void OnNodeAvailabilityChanged(NodeAvailabilityChange availabilityChange) {
      if (availabilityChange == null) {
        throw new ArgumentNullException(nameof(availabilityChange));
      }

      if (availabilityChange.NodeAvailable) {
        ScheduleWork(availabilityChange.NodeId);
      }
    }

    private object scheduleLock;

    private void ScheduleWork(int nodeId) {
      lock (scheduleLock) {
        if (filepathsToTokenize.Any()) {
          var nextFilepath = filepathsToTokenize.Dequeue();
          var assignment = new TokenizationAssignment(nodeId, nextFilepath);

          messageSink.PostTokenizationAssignment(assignment);
          return;
        }

      }
    }
  }
}
