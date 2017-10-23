namespace DocumentClusteringCore.Orchestration.Models {
  public abstract class WorkAssignment {
    public int NodeId { get; }

    public WorkAssignment(int nodeId) {
      NodeId = nodeId;
    }
  }
}
