using System.Collections.Generic;
using CommandLine;

namespace DocumentClusteringAgent {
  public class ConsoleOptions {
    [Option("use-mpi-engine", DefaultValue = false, MutuallyExclusiveSet = "threads",
      HelpText = "Use the MPI engine, instead of the default local threads engine. This option is mutually exclusive with --threads")]
    public bool UseMPIEngine { get; set; }

    [Option("threads", DefaultValue = 0, MutuallyExclusiveSet = "use-mpi-engine",
      HelpText = "Number of local threads to use. Specify 0 to use as much threads as logical cpu cores. This option is mutually exclusive with --use-mpi-engine")]
    public int Threads { get; set; }

    [Option('h', "help", HelpText = "Show help and exit")]
    public bool ShowHelp { get; set; }
    
    [OptionList("files", Required = true, HelpText = "Directory or list of files to read from")]
    public List<string> FilesToRead { get; set; }
  }
}
