namespace DocumentClusteringCore.Orchestration.Models {
  internal abstract class WorkAssignment {
    public int NodeId { get; }

    public WorkAssignment(int nodeId) {
      NodeId = nodeId;
    }
  }
}
