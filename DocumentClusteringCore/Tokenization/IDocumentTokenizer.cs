using System.IO;
using DocumentClusteringCore.Models;

namespace DocumentClusteringCore.Tokenization {
  public interface IDocumentTokenizer {
    Document TokenizeStream(Stream textStream, string name);
  }
}
