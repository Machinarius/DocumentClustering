using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Messaging {
  internal interface IMessageSink {
    void PostDocumentGeneratedMessage(Document document);
  }
}
