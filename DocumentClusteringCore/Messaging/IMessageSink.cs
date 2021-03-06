﻿using System;
using DocumentClusteringCore.Models;
using DocumentClusteringCore.Orchestration.Models;

namespace DocumentClusteringCore.Messaging {
  public interface IMessageSink : IDisposable {
    void PostTokenizedDocument(Document document);
    void PostNormalizedDocument(Document document);
    void PostNodeAvailabilityChange(NodeAvailabilityChange availabilityChange);
    void PostTokenizationAssignment(TokenizationAssignment assignment);
    void PostShutdownAssignment(ShutdownAssignment shutdownAssingment);
    void PostNormalizationAssignment(NormalizationAssignment assignment);
    void PostConfigureNormalizationAssignment(ConfigureNormalizationAssignment assignment);
  }
}
