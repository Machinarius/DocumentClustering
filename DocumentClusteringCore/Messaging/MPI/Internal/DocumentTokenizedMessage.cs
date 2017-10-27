using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Messaging.MPI.Internal {
  public class DocumentTokenizedMessage : DocumentMessage {
    public DocumentTokenizedMessage(Document document) : base(document) { }
  }
}
