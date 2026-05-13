using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordFrequency.V1
{

    class FileReader
    {
        private readonly string _directoryPath = string.Empty;

        public FileReader(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public List<(string FileName, string Content)> ReadAllTextFiles()
        {
            var results = new List<(string, string)>();

            if (!Directory.Exists(_directoryPath))
            {
                Console.WriteLine($"Directory not found: {_directoryPath}");
                return results;
            }

            string[] files = Directory.GetFiles(_directoryPath, "*.txt");

            if (files.Length == 0)
            {
                Console.WriteLine($"No .txt files found in: {_directoryPath}");
                return results;
            }

            foreach (string filePath in files)
            {
                try
                {
                    string content = File.ReadAllText(filePath);
                    results.Add((Path.GetFileName(filePath), content));
                    Console.WriteLine($"Read file: {Path.GetFileName(filePath)}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Could not read {filePath}: {ex.Message}");
                }
            }

            return results;
        }
    }

    class TextProcessor
    {
        private static readonly char[] Separators =
        {
            ' ', '\t', '\n', '\r',
            '.', ',', ';', ':', '!', '?',
            '"', '\'', '(', ')', '[', ']', '{', '}',
            '-', '_', '/', '\\', '@', '#', '*', '&', '%', '+', '=', '<', '>'
        };

        public List<string> GetWords(string text)
        {
            var words = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return words;

            string[] tokens = text.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens)
            {
                string cleaned = token.ToLower().Trim();
                if (!string.IsNullOrEmpty(cleaned))
                    words.Add(cleaned);
            }

            return words;
        }
    }

    class WordCounter
    {
        private readonly Dictionary<string, int> _wordCounts = new Dictionary<string, int>();

        public void AddWords(List<string> words)
        {
            foreach (string word in words)
            {
                if (_wordCounts.ContainsKey(word))
                    _wordCounts[word]++;
                else
                    _wordCounts[word] = 1;
            }
        }

        public void DisplayResults(int topN = 0)
        {
            if (_wordCounts.Count == 0)
            {
                Console.WriteLine("No words found.");
                return;
            }

            var sorted = _wordCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .ToList();

            if (topN > 0)
                sorted = sorted.Take(topN).ToList();

            Console.WriteLine($"\n{"WORD",-25} {"COUNT",8}");
            Console.WriteLine(new string('-', 35));

            foreach (KeyValuePair<string, int> kv in sorted)
                Console.WriteLine($"{kv.Key,-25} {kv.Value,8}");

            Console.WriteLine(new string('-', 35));
            Console.WriteLine($"Total unique words:     {_wordCounts.Count}");
            Console.WriteLine($"Total word occurrences: {_wordCounts.Values.Sum()}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Word Frequency Counter  -  V1");

            if (args.Length < 1)
            {
                Console.WriteLine("Enter directory path:");
                string directoryPath2 = Console.ReadLine() ?? string.Empty;
                args = new string[] { directoryPath2 };
            }

            string directoryPath = args[0];
            int topN = 0;

            if (args.Length >= 2 && (!int.TryParse(args[1], out topN) || topN < 0))
            {
                Console.WriteLine("Invalid top_n value; showing all words.");
                topN = 0;
            }

            var reader = new FileReader(directoryPath);
            var processor = new TextProcessor();
            var counter = new WordCounter();

            Console.WriteLine($"Scanning directory: {directoryPath}\n");
            var files = reader.ReadAllTextFiles();

            if (files.Count == 0)
            {
                Console.WriteLine("Nothing to process. Exiting.");
                return;
            }

            foreach (var (_, content) in files)
                counter.AddWords(processor.GetWords(content));

            Console.WriteLine($"\nFiles processed: {files.Count}");
            Console.WriteLine(topN > 0 ? $"Showing top {topN} words:\n" : "Showing all words:\n");

            counter.DisplayResults(topN);

            Console.WriteLine("\nDone. Press any key to exit.");
            Console.ReadKey();


            Console.ReadLine();
        }
    }
}
