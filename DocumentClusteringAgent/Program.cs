using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentClusteringCore.Configuration;
using DocumentClusteringCore.Messaging.InProcess;
using DocumentClusteringCore.Normalization.Default;
using DocumentClusteringCore.Orchestration;
using DocumentClusteringCore.Orchestration.LocalThreads;
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
      if (args.Length < 1) {
        Console.WriteLine($"Please specify the path to the directory or file to analyze as an argument");
        return;
      }

      var pathToTarget = Path.GetFullPath(args[0]);
      if (!File.Exists(pathToTarget) && !Directory.Exists(pathToTarget)) {
        Console.WriteLine($"ERROR: No file or directory was found at '{pathToTarget}'");
        Console.ReadKey();
        return;
      }

      IEnumerable<string> targetFiles;
      var targetIsDirectory = (File.GetAttributes(pathToTarget) & FileAttributes.Directory) == FileAttributes.Directory;
      if (targetIsDirectory) {
        targetFiles = Directory.GetFiles(pathToTarget).Select(Path.GetFullPath).ToArray();
      } else {
        targetFiles = new[] { pathToTarget }.Select(Path.GetFullPath).ToArray();
      }

      var options = new Options(0, targetFiles);

      var containerRules = Rules.Default
        .WithConcreteTypeDynamicRegistrations()
        .WithAutoConcreteTypeResolution();

      var diContainer = new Container(containerRules);
      diContainer.UseInProcessMessaging();
      diContainer.UseDefaultTokenization();
      diContainer.UseCachedPorterStemming();
      diContainer.UseLocalThreadWorkers();
      diContainer.UseDefaultTermFiltering();
      diContainer.UseEuclideanSimilarityComparison();
      diContainer.UseDefaultWeightNormalization();
      diContainer.RegisterInstance(options);

      var worker = diContainer.Resolve<IWorkOrchestrator>();
      var stopWatch = new Stopwatch();

      stopWatch.Start();
      await worker.ExecuteWorkAsync();
      stopWatch.Stop();

      Console.WriteLine($"Time ellapsed: {stopWatch.Elapsed.TotalSeconds} seconds");
      
      Console.ReadKey();
    }
  }
}
