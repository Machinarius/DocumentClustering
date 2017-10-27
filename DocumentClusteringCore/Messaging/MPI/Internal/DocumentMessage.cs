using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Messaging.MPI.Internal {
  public abstract class DocumentMessage : INodeMessage {
    public int NodeId => 0; // Always send this message to the root
    public Document Document { get; }

    public DocumentMessage(Document document) {
      Document = document ?? throw new System.ArgumentNullException(nameof(document));
    }
  }
}
