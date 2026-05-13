using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordFrequency.V2
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

        private readonly int _n;
        private readonly int _m; 

        public TextProcessor(int n = 6, int m = 2)
        {
            _n = n < 1 ? 1 : n;
            _m = m < 1 ? 1 : m;
        }

        public List<string> GetWords(string text)
        {
            var words = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return words;

            string[] tokens = text.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens)
            {
                string cleaned = token.ToLower().Trim();

                if (string.IsNullOrEmpty(cleaned))
                    continue;


                if (cleaned.Length > _n)
                {
                    int newLength = cleaned.Length - _m;
                    cleaned = newLength >= 1 ? cleaned.Substring(0, newLength) : cleaned.Substring(0, 1);
                }

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

            Console.WriteLine($"\n{"WORD (trimmed)",-25} {"COUNT",8}");
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
            Console.WriteLine("Word Frequency Counter  -  V2");
            Console.WriteLine("(with suffix trimming)");

            if (args.Length < 1)
            {
                Console.WriteLine("Enter directory path:");
                string directoryPath2 = Console.ReadLine() ?? string.Empty;
                args = new string[] { directoryPath2 };
            }

            string directoryPath = args[0];
            int n = ParseArg(args, 1, defaultValue: 6, "N");
            int m = ParseArg(args, 2, defaultValue: 2, "M");
            int topN = ParseArg(args, 3, defaultValue: 0, "top_n");

            Console.WriteLine($"Settings: N={n} (min length to trim), M={m} (chars to remove)\n");

            var reader = new FileReader(directoryPath);
            var processor = new TextProcessor(n, m);
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
            Console.WriteLine($"Suffix trimming: words longer than {n} chars -> last {m} chars removed");
            Console.WriteLine(topN > 0 ? $"Showing top {topN} words:\n" : "Showing all words:\n");

            counter.DisplayResults(topN);

            Console.WriteLine("\nDone. Press any key to exit.");
            Console.ReadKey();
            Console.ReadLine();
        }

        static int ParseArg(string[] args, int index, int defaultValue, string label)
        {
            if (args.Length <= index) return defaultValue;
            if (int.TryParse(args[index], out int value) && value >= 0) return value;
            Console.WriteLine($"Invalid {label} value '{args[index]}'; using default {defaultValue}.");
            return defaultValue;


        } 
    }
}
