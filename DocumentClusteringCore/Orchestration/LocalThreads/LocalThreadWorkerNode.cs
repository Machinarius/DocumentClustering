using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using DocumentClusteringCore.Messaging;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Normalization;
using DocumentClusteringCore.Orchestration.Models;
using DocumentClusteringCore.SimilarityComparison;
using DocumentClusteringCore.Tokenization;

namespace DocumentClusteringCore.Orchestration.LocalThreads {
  public class LocalThreadWorkerNode {
    private readonly int id;

    private readonly IMessageHub messageHub;
    private readonly IMessageSink messageSink;

    private readonly IDocumentTokenizer documentTokenizer;
    private readonly IWeightNormalizer weightNormalizer;
    private readonly ISimilarityComparer similarityComparer;

    private readonly IObservable<WorkAssignment> assignments;

    private Thread workerThread;
    private bool keepWorking;

    public LocalThreadWorkerNode(int id, IMessageHub messageHub, IMessageSink messageSink,
                                 IDocumentTokenizer documentTokenizer, IWeightNormalizer weightNormalizer,
                                 ISimilarityComparer similarityComparer) {
      this.id = id;

      this.messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
      this.messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
      this.documentTokenizer = documentTokenizer ?? throw new ArgumentNullException(nameof(documentTokenizer));
      this.weightNormalizer = weightNormalizer ?? throw new ArgumentNullException(nameof(weightNormalizer));
      this.similarityComparer = similarityComparer ?? throw new ArgumentNullException(nameof(similarityComparer));
      
      workerThread = new Thread(WorkLoop) {
        Name = "Worker_" + id
      };

      assignments = messageHub.WorkAssignemnts
        .Where(assignment => assignment.NodeId == id);
    }

    public void Start() {
      keepWorking = true;
      workerThread.Start();
    }

    public void Stop() {
      keepWorking = false;
      workerThread.Join();
    }

    private void WorkLoop() {
      NodeAvailabilityChange availabilityChange;

      var assignmentsStream = assignments.ToEnumerable();

      availabilityChange = new NodeAvailabilityChange(id, true);
      messageSink.PostNodeAvailabilityChange(availabilityChange);

      foreach (var assignment in assignmentsStream) {
        if (!keepWorking) {
          return;
        }

        if (assignment == null) {
          throw new InvalidOperationException("Cannot execute a null assignment");
        }
        
        availabilityChange = new NodeAvailabilityChange(id, false);
        messageSink.PostNodeAvailabilityChange(availabilityChange);

        switch (assignment) {
          case TokenizationAssignment tokenization:
            ExecuteTokenization(tokenization);
            break;
          case NormalizationAssignment normalizaton:
            ExecuteNormalization(normalizaton);
            break;
          default:
            throw new InvalidOperationException("Did not recognize the assignment type: " + assignment.GetType());
        }
        
        availabilityChange = new NodeAvailabilityChange(id, true);
        messageSink.PostNodeAvailabilityChange(availabilityChange);
      }
    }

    private void ExecuteTokenization(TokenizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }
      
      if (!File.Exists(assignment.Filepath)) {
        throw new InvalidOperationException("Cannot tokenize a file that does not exist: " + assignment.Filepath);
      }

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

      weightNormalizer.NormalizeDocument(assignment.NormalizationSubject);
      messageSink.PostNormalizedDocument(assignment.NormalizationSubject);
    }
  }
}
