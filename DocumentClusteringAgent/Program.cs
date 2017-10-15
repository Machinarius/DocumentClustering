using System;
using System.IO;
using DocumentClusteringCore.DocumentParsing;
using DocumentClusteringCore.DocumentParsing.Implementations;
using DocumentClusteringCore.Stemming;
using DocumentClusteringCore.Stemming.Implementations;
using DocumentClusteringCore.TermFiltering;
using DocumentClusteringCore.TermFiltering.Implementations;
using DryIoc;

namespace DocumentClusteringAgent {
  public static class Program {
    public static void Main(string[] args) {
      if (args.Length < 1) {
        Console.WriteLine($"Please specify the path to the file to analyze as an argument");
        return;
      }

      var pathToFile = Path.GetFullPath(args[0]);
      if (!File.Exists(pathToFile)) {
        Console.WriteLine($"ERROR: The file '{pathToFile}' does not exist");
        Console.ReadKey();
        return;
      }

      Stream fileStream;
      try {
        fileStream = File.OpenRead(pathToFile);
      } catch (Exception) {
        Console.WriteLine($"ERROR: The file '{pathToFile}' could not be read");
        Console.ReadKey();
        return;
      }

      var diContainer = new Container();
      diContainer.Register<IDocumentFactory, DefaultDocumentFactory>();
      diContainer.Register<ITermSieve, DefaultTermSieve>();

      var stemmer = new CachedWordStemmer(new PorterWordStemmer());
      diContainer.RegisterInstance<IWordStemmer>(stemmer);

      var docFactory = diContainer.Resolve<IDocumentFactory>();
      var docName = Path.GetFileName(pathToFile);
      var document = docFactory.ParseStream(fileStream, docName);

      Console.WriteLine($"Terms and weights for file: {docName}");
      foreach (var termWeight in document.TermWeights) {
        Console.WriteLine($"{termWeight.Key}: {termWeight.Value}");
      }

      Console.WriteLine();
      Console.ReadKey();
    }
  }
}
