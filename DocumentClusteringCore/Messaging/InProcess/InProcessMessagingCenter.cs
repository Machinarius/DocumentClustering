using System;
using System.Reactive.Subjects;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Messaging.InProcess {
  public class InProcessMessagingCenter : IMessageHub, IMessageSink {
    public IObservable<Document> DocumentGenerated => documentGeneratedSubject;
    public IObservable<Document> DocumentNormalized => documentNormalizedSubject;
    public IObservable<WorkAssignment> WorkAssignemnts => assignmentsSubject;
    public IObservable<NodeAvailabilityChange> NodeAvailabilityChanges => availabilityChangesSubject;

    private Subject<Document> documentGeneratedSubject;
    private Subject<WorkAssignment> assignmentsSubject;
    private Subject<Document> documentNormalizedSubject;
    private Subject<NodeAvailabilityChange> availabilityChangesSubject;

    public InProcessMessagingCenter() {
      documentGeneratedSubject = new Subject<Document>();
      documentNormalizedSubject = new Subject<Document>();
      assignmentsSubject = new Subject<WorkAssignment>();
      availabilityChangesSubject = new Subject<NodeAvailabilityChange>();
    }

    public void PostTokenizedDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      documentGeneratedSubject.OnNext(document);
    }

    public void PostNormalizedDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      documentNormalizedSubject.OnNext(document);
    }

    public void PostNodeAvailabilityChange(NodeAvailabilityChange availabilityChange) {
      if (availabilityChange == null) {
        throw new ArgumentNullException(nameof(availabilityChange));
      }

      availabilityChangesSubject.OnNext(availabilityChange);
    }

    public void PostTokenizationAssignment(TokenizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      assignmentsSubject.OnNext(assignment);
    }

    public void PostShutdownAssignment(ShutdownAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      assignmentsSubject.OnNext(assignment);
    }

    public void PostNormalizationAssignment(NormalizationAssignment assignment) {
      if (assignment == null) {
        throw new ArgumentNullException(nameof(assignment));
      }

      assignmentsSubject.OnNext(assignment);
    }
  }
}
