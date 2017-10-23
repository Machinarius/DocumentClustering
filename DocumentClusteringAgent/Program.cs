using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DocumentClusteringCore.Tokenization;
using DocumentClusteringCore.Tokenization.Implementations;
using DocumentClusteringCore.Stemming;
using DocumentClusteringCore.Stemming.Implementations;
using DocumentClusteringCore.TermFiltering;
using DocumentClusteringCore.TermFiltering.Implementations;
using DryIoc;

namespace DocumentClusteringAgent {
  public static class Program {
    public static void Main(string[] args) {
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
        targetFiles = Directory.GetFiles(pathToTarget);
      } else {
        targetFiles = new[] { pathToTarget };
      }

      var targets = new List<(Stream stream, string name)>();
      foreach (var filePath in targetFiles) {
        Stream fileStream;
        try {
          fileStream = File.OpenRead(filePath);
        } catch (Exception) {
          Console.WriteLine($"ERROR: The file '{pathToTarget}' could not be read");
          Console.ReadKey();
          return;
        }

        var docName = Path.GetFileName(filePath);
        targets.Add((fileStream, docName));
      }

      var diContainer = new Container();
      diContainer.Register<IDocumentTokenizer, DefaultDocumentFactory>();
      diContainer.Register<ITermSieve, DefaultTermSieve>();

      var stemmer = new CachedWordStemmer(new PorterWordStemmer());
      diContainer.RegisterInstance<IWordStemmer>(stemmer);

      var docFactory = diContainer.Resolve<IDocumentTokenizer>();

      var stopWatch = new Stopwatch();
      stopWatch.Start();
      foreach (var target in targets) {
        var document = docFactory.TokenizeStream(target.stream, target.name);

        Console.WriteLine($"Terms and weights for file: {target.name}");
        foreach (var termCount in document.NormalizedTermWeights) {
          Console.WriteLine($"{termCount.Key}: {termCount.Value}");
        }
      }
      stopWatch.Stop();

      Console.WriteLine();
      Console.WriteLine($"Document count: {targets.Count}");
      Console.WriteLine($"Time ellapsed: {stopWatch.Elapsed.TotalSeconds} seconds");
      
      Console.ReadKey();
    }
  }
}
