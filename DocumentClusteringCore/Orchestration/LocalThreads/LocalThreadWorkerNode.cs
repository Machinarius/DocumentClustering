using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Normalization;
using DocumentClusteringCore.Orchestration.Models;
using DocumentClusteringCore.SimilarityComparison;
using DocumentClusteringCore.Tokenization;
using Nito.AsyncEx;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class LocalThreadWorkerNode : IWorkerNode {
    public int Id { get; }

    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;

    private readonly IDocumentTokenizer documentTokenizer;
    private readonly IWeightNormalizer weightNormalizer;
    private readonly ISimilarityComparer similarityComparer;

    private readonly IObservable<WorkAssignment> assignments;
    
    private readonly TaskCompletionSource<int> workIsDoneTCS;

    private Thread workerThread;
    private IDisposable assignmentsSubscription;

    public LocalThreadWorkerNode(int id, IMessageHub messageHub, IMessageSink messageSink,
                                 IDocumentTokenizer documentTokenizer, IWeightNormalizer weightNormalizer,
                                 ISimilarityComparer similarityComparer) {
      Id = id;

      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.documentTokenizer = documentTokenizer ?? throw new ArgumentNullException(nameof(documentTokenizer));
      this.weightNormalizer = weightNormalizer ?? throw new ArgumentNullException(nameof(weightNormalizer));
      this.similarityComparer = similarityComparer ?? throw new ArgumentNullException(nameof(similarityComparer));

      workIsDoneTCS = new TaskCompletionSource<int>();

      assignments = messageHub.WorkAssignemnts
        .Where(assignment => assignment.NodeId == id);
    }

    private void StartWork() {
      AsyncContext.Run(WaitUntilWorkIsDoneAsync);
    }

    private async Task WaitUntilWorkIsDoneAsync() {
      assignmentsSubscription = assignments
        .ObserveOn(Scheduler.Immediate)
        .Subscribe(ExecuteAssignment);

      PostAvailability(true);

      using (assignmentsSubscription) {
        await workIsDoneTCS.Task;
      }
    }

    private void ExecuteAssignment(WorkAssignment assignment) {
      PostAvailability(false);

      switch (assignment) {
        case TokenizationAssignment tokenization:
          ExecuteTokenization(tokenization);
          break;
        case NormalizationAssignment normalizaton:
          ExecuteNormalization(normalizaton);
          break;
        case ShutdownAssignment shutdown:
          ExecuteShutdown(shutdown);
          break;
        default:
          throw new InvalidOperationException("Did not recognize the assignment type: " + assignment.GetType());
      }

      PostAvailability(true);
    }

    private void ExecuteShutdown(ShutdownAssignment shutdown) {
      if (shutdown == null) {
        throw new ArgumentNullException(nameof(shutdown));
      }

      workIsDoneTCS.TrySetResult(0);
    }

    private void ExecuteTokenization(TokenizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }
      
      if (!File.Exists(assignment.Filepath)) {
        throw new InvalidOperationException("Cannot tokenize a file that does not exist: " + assignment.Filepath);
      }
      
      Debug.WriteLine($"Node {Id} - Running tokenization on {assignment.Filepath}");

      Document tokenizedDocument;
      using (var fileStream = File.OpenRead(assignment.Filepath)) {
        var filename = Path.GetFileName(assignment.Filepath);
        tokenizedDocument = documentTokenizer.TokenizeStream(fileStream, filename);
      }

      messageSink.PostTokenizedDocument(tokenizedDocument);
    }

    private void ExecuteNormalization(NormalizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      Debug.WriteLine($"Node {Id} - Running normalization on {assignment.NormalizationSubject.Name}");
      weightNormalizer.NormalizeDocument(assignment.NormalizationSubject);
      messageSink.PostNormalizedDocument(assignment.NormalizationSubject);
    }

    private void PostAvailability(bool available) {
      var availabilityChange = new NodeAvailabilityChange(Id, available);
      messageSink.PostNodeAvailabilityChange(availabilityChange);
    }

    public Task StartAsync() {
      workerThread = new Thread(StartWork) {
        Name = "Worker_" + Id
      };
      workerThread.Start();

      return Task.FromResult(0);
    }

    public Task StopAsync() {
      workIsDoneTCS.TrySetResult(0);
      workerThread.Join();

      return Task.FromResult(0);
    }
  }
}
