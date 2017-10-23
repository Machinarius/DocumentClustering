using System;
using System.Reactive.Subjects;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Messaging.Implementations {
  public class DefaultMessagingCenter : IMessageHub, IMessageSink {
    public IObservable<Document> DocumentGenerated => documentGeneratedSubject;

    private Subject<Document> documentGeneratedSubject;

    public DefaultMessagingCenter() {
      documentGeneratedSubject = new Subject<Document>();
    }

    public void PostTokenizedDocument(Document document) {
      if (document == null) {
        throw new ArgumentNullException(nameof(document));
      }

      documentGeneratedSubject.OnNext(document);
    }
  }
}
