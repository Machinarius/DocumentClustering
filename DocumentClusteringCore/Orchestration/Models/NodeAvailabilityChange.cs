namespace DocumentClusteringCore.Orchestration.Models {
  public class NodeAvailabilityChange : INodeMessage {
    public int NodeId { get; }
    public bool NodeAvailable { get; }

    public NodeAvailabilityChange(int nodeId, bool nodeAvailable) {
      NodeId = nodeId;
      NodeAvailable = nodeAvailable;
    }
  }
}
