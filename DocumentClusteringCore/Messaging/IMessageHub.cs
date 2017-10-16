using System;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Messaging {
  public interface IMessageHub {
    IObservable<Document> DocumentGenerated { get; }
  }
}
