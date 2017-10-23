using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Messaging {
  public interface IMessageSink {
    void PostTokenizedDocument(Document document);
    void PostNormalizedDocument(Document document);
    void PostNodeAvailabilityChange(NodeAvailabilityChange availabilityChange);
    void PostTokenizationAssignment(TokenizationAssignment assignment);
  }
}
