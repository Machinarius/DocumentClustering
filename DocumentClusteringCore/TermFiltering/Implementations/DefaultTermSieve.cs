using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentClusteringCore.Stemming;

namespace DocumentClusteringCore.TermFiltering.Implementations {
  public class DefaultTermSieve : ITermSieve {
    private const int BufferSizeInKB = 4096;

    private IWordStemmer stemmer;

    public DefaultTermSieve(IWordStemmer stemmer) {
      this.stemmer = stemmer ?? throw new ArgumentNullException(nameof(stemmer));
    }

    public IEnumerable<string> GetTextTerms(Stream textStream) {
      if (textStream == null) {
        throw new ArgumentNullException(nameof(textStream));
      }

      var words = new List<string>();
      var wordStream = new WordStream();
      wordStream.WordGenerated += (source, generatedWord) => {
        words.Add(generatedWord);
      };

      var fileReadEntirely = false;
      var buffer = new byte[BufferSizeInKB];
      while (!fileReadEntirely) {
        var readBytes = textStream.Read(buffer, 0, BufferSizeInKB);
        fileReadEntirely = readBytes != BufferSizeInKB;

        foreach (var textByte in buffer) {
          var textCharacter = (char)textByte;
          wordStream.AddCharacter(textCharacter);
        }
      }

      var filteredWords = words.Except(WordsBlacklist.Instance);
      var terms = filteredWords.Select(stemmer.StemString).ToList().AsEnumerable();

      return terms;
    }

    private class WordStream {
      private const int UppercaseACode = 65;
      private const int UppercaseZCode = 90;

      private const int LowercaseACode = 97;
      private const int LowercaseZCode = 122;

      public event EventHandler<string> WordGenerated;

      private List<char> currentWord;

      public WordStream() {
        currentWord = new List<char>();
      }

      public void AddCharacter(char character) {
        if (IsValidWordChar(character)) {
          currentWord.Add(character);
        } else {
          if (currentWord.Any()) {
            var generatedWord = new String(currentWord.ToArray());
            WordGenerated?.Invoke(this, generatedWord);

            currentWord.Clear();
          }
        }
      }

      private bool IsValidWordChar(char character) {
        var charCode = (int)character;
        return
          (UppercaseACode <= charCode && charCode <= UppercaseZCode) ||
          (LowercaseACode <= charCode && charCode <= LowercaseZCode);
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
