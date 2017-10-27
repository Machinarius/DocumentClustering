using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging.InProcess;
using DocumentClusteringCore.Messaging.MPI;
using DocumentClusteringCore.Normalization.Default;
using DocumentClusteringCore.Orchestration;
using DocumentClusteringCore.Orchestration.LocalThreads;
using DocumentClusteringCore.Orchestration.MPI;
using DocumentClusteringCore.SimilarityComparison.Euclidean;
using DocumentClusteringCore.Stemming.Default;
using DocumentClusteringCore.TermFiltering.Default;
using DocumentClusteringCore.Tokenization.Default;
using DryIoc;
using Nito.AsyncEx;

namespace DocumentClusteringAgent {
  public static class Program {
    public static void Main(string[] args) {
      Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

      AsyncContext.Run(() => AsyncMain(args));
    }

    public static async Task AsyncMain(string[] args) {
      var consoleOptions = new ConsoleOptions();
      var optionsAreValid = CommandLine.Parser.Default.ParseArguments(args, consoleOptions);

      consoleOptions.FilesToRead = consoleOptions.FilesToRead?.Select(Path.GetFullPath).ToList();

      if (!optionsAreValid || consoleOptions.ShowHelp || 
          !consoleOptions.FilesToRead.All(x => File.Exists(x) || Directory.Exists(x))) {
        ShowHelp(consoleOptions);

        return;
      }

      var filesToRead = consoleOptions.FilesToRead;
      if (filesToRead.Count == 1) {
        var directoryName = filesToRead[0];
        if (Directory.Exists(directoryName)) {
          filesToRead = Directory.GetFiles(directoryName).ToList();
        }
      }

      var options = new Options(consoleOptions.Threads, filesToRead);

      var containerRules = Rules.Default
        .WithConcreteTypeDynamicRegistrations()
        .WithAutoConcreteTypeResolution();

      var diContainer = new Container(containerRules);
      diContainer.RegisterInstance(options);

      diContainer.UseDefaultTokenization();
      diContainer.UseCachedPorterStemming();
      diContainer.UseDefaultTermFiltering();
      diContainer.UseEuclideanSimilarityComparison();
      diContainer.UseDefaultWeightNormalization();

      IDisposable mpiEnvironment = null;

      if (consoleOptions.UseMPIEngine) {
        mpiEnvironment = new MPI.Environment(ref args);

        diContainer.UseMPIMessaging();
        diContainer.UseMPIOrchestration();
      } else {
        diContainer.UseInProcessMessaging();
        diContainer.UseLocalThreadWorkers();
      }

      var worker = diContainer.Resolve<IWorkOrchestrator>();
      var stopWatch = new Stopwatch();

      stopWatch.Start();
      await worker.ExecuteWorkAsync();
      stopWatch.Stop();

      if (mpiEnvironment != null) {
        mpiEnvironment.Dispose();
      }

      Console.WriteLine($"Time ellapsed: {stopWatch.Elapsed.TotalSeconds} seconds");
      
      Console.ReadKey();
    }

    private static void ShowHelp(ConsoleOptions consoleOptions) {
      var helpText = CommandLine.Text.HelpText.AutoBuild(consoleOptions);
      Console.WriteLine(helpText);
    }
  }
}
