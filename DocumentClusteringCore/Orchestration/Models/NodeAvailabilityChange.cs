namespace DocumentClusteringCore.Orchestration.Models {
  public class NodeAvailabilityChange {
    public int NodeId { get; }
    public bool NodeAvailable { get; }

    public NodeAvailabilityChange(int nodeId, bool nodeAvailable) {
      NodeId = nodeId;
      NodeAvailable = nodeAvailable;
    }
  }
}
