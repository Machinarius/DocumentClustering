using System.Collections.Generic;
using System.IO;

namespace DocumentClusteringCore.TermFiltering {
  public interface ITermSieve {
    IEnumerable<string> GetTextTerms(Stream textStream);
  }
}
