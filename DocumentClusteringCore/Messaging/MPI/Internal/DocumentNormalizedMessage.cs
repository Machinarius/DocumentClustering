using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Messaging.MPI.Internal {
  public class DocumentNormalizedMessage : DocumentMessage {
    public DocumentNormalizedMessage(Document document) : base(document) { }
  }
}
