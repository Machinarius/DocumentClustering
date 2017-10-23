using System.Collections.Generic;

namespace DocumentClusteringCore.Configuration {
  public class Options {
    public bool UseMPIWorkers { get; }
    public IEnumerable<string> Filenames { get; }
    public int NodeCount { get; }
  }
}
