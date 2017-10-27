using System;
using System.Collections.Concurrent;
using System.Threading;
using DocumentClusteringCore.Messaging.MPI.Internal;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;
using MPI;

namespace DocumentClusteringCore.Messaging.MPI {
  public class MPIMessageSink : IMessageSink {
    private const int RootNode = 0;

    private readonly Intracommunicator communicator;
    private readonly MPIMessageSerializer messageSerializer;

    private readonly ConcurrentQueue<INodeMessage> nodeMessages;
    private readonly Thread sendThread;

    private bool keepRunning;

    public MPIMessageSink() {
      communicator = Communicator.world;
      messageSerializer = new MPIMessageSerializer();

      nodeMessages = new ConcurrentQueue<INodeMessage>();
      sendThread = new Thread(SendLoop) {
        Name = "MessageSendThread"
      };
      
      keepRunning = true;
      sendThread.Start();
    }

    public void Dispose() {
      keepRunning = false;
      sendThread.Join();
    }

    private void SendLoop() {
      while(keepRunning) {
        INodeMessage message;
        while (!nodeMessages.TryDequeue(out message)) {
          if (!keepRunning) {
            return;
          }
        }

        if (message == null) {
          throw new InvalidOperationException("Cannot write a null message");
        }

        var targetNode = message.NodeId;
        if (message is NodeAvailabilityChange change) {
          targetNode = RootNode;
        }

        var serializedMessage = messageSerializer.SerializeMessage(message);
        communicator.Send(serializedMessage, targetNode, MPIMessageTags.SerializedMessagesTag);
      }
    }

    public void PostConfigureNormalizationAssignment(ConfigureNormalizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      nodeMessages.Enqueue(assignment);
    }

    public void PostNodeAvailabilityChange(NodeAvailabilityChange availabilityChange) {
      if (availabilityChange == null) {
        throw new ArgumentNullException(nameof(availabilityChange));
      }

      nodeMessages.Enqueue(availabilityChange);
    }

    public void PostNormalizationAssignment(NormalizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      nodeMessages.Enqueue(assignment);
    }

    public void PostNormalizedDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      var message = new DocumentNormalizedMessage(document);
      nodeMessages.Enqueue(message);
    }

    public void PostShutdownAssignment(ShutdownAssignment shutdownAssingment) {
      if (shutdownAssingment == null) {
        throw new ArgumentNullException(nameof(shutdownAssingment));
      }

      nodeMessages.Enqueue(shutdownAssingment);
    }

    public void PostTokenizationAssignment(TokenizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      nodeMessages.Enqueue(assignment);
    }

    public void PostTokenizedDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      var message = new DocumentTokenizedMessage(document);
      nodeMessages.Enqueue(message);
    }
  }
}
