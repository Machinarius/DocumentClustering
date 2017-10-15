using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using DocumentClusteringCore.Stemming;

namespace DocumentClusteringCore.TermFiltering.Implementations {
  public class DefaultTermSieve : ITermSieve {
    private const int BufferSizeInKB = 4096;

    private IWordStemmer stemmer;

    public DefaultTermSieve(IWordStemmer stemmer) {
      this.stemmer = stemmer ?? throw new ArgumentNullException(nameof(stemmer));
    }

    // This will be paralelized so loading the stream into memory by chunks is of utmost importance
    public IEnumerable<string> GetTextTerms(Stream textStream) {
      if (textStream == null) {
        throw new ArgumentNullException(nameof(textStream));
      }

      string text;
      using (var textReader = new StreamReader(textStream)) {
        text = textReader.ReadToEnd();
      }

      var wordMatches = WordRegex.Instance.Matches(text);
      var words = new List<string>();

      foreach (Match wordMatch in wordMatches) {
        var word = wordMatch.Value;
        words.Add(word);
      }

      var filteredWords = words.Except(WordsBlacklist.Instance);
      var terms = filteredWords.Select(stemmer.StemString).ToList().AsEnumerable();

      return terms;
    }

    private static class WordRegex {
      private static Regex _instance;
      public static Regex Instance {
        get {
          if (_instance == null) {
            _instance = new Regex("([a-z]|[A-Z])+");
          }

          return _instance;
        }
      }
    }

    private static class WordsBlacklist {
      private static HashSet<string> _instance;
      public static HashSet<string> Instance {
        get {
          if (_instance == null) {
            var resourceId = typeof(DefaultTermSieve).Namespace + ".WordsBlacklist.txt";
            var resourceAssembly = typeof(DefaultTermSieve).Assembly;

            var resourceStream = resourceAssembly.GetManifestResourceStream(resourceId);
            if (resourceStream == null) {
              throw new InvalidOperationException("Could not read the blacklist file. Id: " + resourceId);
            }

            string[] blacklistLines;
            using (resourceStream) {
              using (var resourceReader = new StreamReader(resourceStream)) {
                var blacklistContents = resourceReader.ReadToEnd();
                blacklistLines = blacklistContents.Split('\n');
              }
            }

            var blackList = new HashSet<string>();
            foreach (var line in blacklistLines) {
              var word = line.Trim().TrimStart('\r').TrimEnd('\n');
              blackList.Add(word);
            }

            _instance = blackList;
          }

          return _instance;
        }
      }
    }
  }
}
