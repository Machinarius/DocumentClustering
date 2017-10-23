using System;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Messaging {
  public interface IMessageHub {
    IObservable<Document> DocumentGenerated { get; }
    IObservable<NodeAvailabilityChange> NodeAvailabilityChanges { get; }
    IObservable<WorkAssignment> WorkAssignemnts { get; }
  }
}
