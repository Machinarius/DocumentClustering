using System;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Messaging {
  public interface IMessageHub : IDisposable {
    IObservable<Document> DocumentTokenized { get; }
    IObservable<Document> DocumentNormalized { get; }
    IObservable<WorkAssignment> WorkAssignemnts { get; }
    IObservable<NodeAvailabilityChange> NodeAvailabilityChanges { get; }
  }
}
