using System.IO;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.DocumentParsing {
  public interface IDocumentFactory {
    Document ParseStream(Stream textStream, string name);
  }
}
