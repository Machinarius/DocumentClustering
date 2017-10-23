using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentClusteringCore.Configuration {
  public class Options {
    public int NodeCount { get; }
    public IEnumerable<string> FilePaths { get; }

    public Options(int nodeCount, IEnumerable<string> filePaths) {
      NodeCount = nodeCount;
      FilePaths = filePaths ?? throw new ArgumentNullException(nameof(filePaths));

      if (!FilePaths.Any()) {
        throw new InvalidOperationException("No files to work on");
      }
    }
  }
}
