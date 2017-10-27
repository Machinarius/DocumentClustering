using System;
using Newtonsoft.Json;

namespace DocumentClusteringCore.Messaging.MPI.Internal {
  public class MPIMessageSerializer {
    private JsonSerializerSettings settings;

    public MPIMessageSerializer() {
      settings = new JsonSerializerSettings {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.None
      };
    }

    public string SerializeMessage(object messageObject) {
      if (messageObject == null) {
        throw new ArgumentNullException(nameof(messageObject));
      }

      return JsonConvert.SerializeObject(messageObject, settings);
    }

    public TMessage DeserializeMessage<TMessage>(string serializedMessage) {
      if (string.IsNullOrEmpty(serializedMessage)) {
        throw new ArgumentNullException(nameof(serializedMessage));
      }

      return JsonConvert.DeserializeObject<TMessage>(serializedMessage, settings);
    }
  }
}
