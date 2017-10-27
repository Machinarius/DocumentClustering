using System;
using System.Reactive.Subjects;
using System.Threading;
using DocumentClusteringCore.Messaging.MPI.Internal;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;
using MPI;

namespace DocumentClusteringCore.Messaging.MPI {
  public class MPIMessageHub : IMessageHub {
    private const int RootNode = 0;

    public IObservable<Document> DocumentTokenized => documentTokenizedSubject;
    public IObservable<Document> DocumentNormalized => documentNormalizedSubject;
    public IObservable<WorkAssignment> WorkAssignemnts => assignmentsSubject;
    public IObservable<NodeAvailabilityChange> NodeAvailabilityChanges => availabilityChangesSubject;

    private readonly Subject<Document> documentTokenizedSubject;
    private readonly Subject<WorkAssignment> assignmentsSubject;
    private readonly Subject<Document> documentNormalizedSubject;
    private readonly Subject<NodeAvailabilityChange> availabilityChangesSubject;

    private readonly MPIMessageSerializer messageSerializer;
    private readonly Intracommunicator communicator;
    private readonly Thread readThread;

    private bool keepReading;

    public MPIMessageHub() {
      documentTokenizedSubject = new Subject<Document>();
      documentNormalizedSubject = new Subject<Document>();
      assignmentsSubject = new Subject<WorkAssignment>();
      availabilityChangesSubject = new Subject<NodeAvailabilityChange>();

      messageSerializer = new MPIMessageSerializer();
      communicator = Communicator.world;
      keepReading = true;

      readThread = new Thread(ReadMessagesLoop) {
        Name = "ReadMessagesThread"
      };
      readThread.Start();
    }

    public void Dispose() {
      keepReading = false;
      readThread.Join();
    }

    private void ReadMessagesLoop() {
      while (keepReading) {
        var messageString = communicator.Receive<string>(RootNode, MPIMessageTags.SerializedMessagesTag);
        var message = messageSerializer.DeserializeMessage<INodeMessage>(messageString);

        switch (message) {
          case DocumentTokenizedMessage tokenizedMessage:
            documentTokenizedSubject.OnNext(tokenizedMessage.Document);
            break;
          case DocumentNormalizedMessage normalizedMessage:
            documentNormalizedSubject.OnNext(normalizedMessage.Document);
            break;
          case NodeAvailabilityChange availabilityChange:
            availabilityChangesSubject.OnNext(availabilityChange);
            break;
          case WorkAssignment assignment:
            assignmentsSubject.OnNext(assignment);
            break;
        }
      }
    }
  }
}
