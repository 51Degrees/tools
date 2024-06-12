using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CopyrightUpdater
{
    class Program
    {
        private enum Command
        {
            Report,
            Enforce
        }

        private static Config ReadConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", true, true)
              .Build();
            Config result = new Config();
            config.Bind(result);
            return result;
        }

        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Expected argument format: [command] [path]");
                Console.WriteLine("command = -r (Report) or -e (Enforce)");
                Console.WriteLine("path = root directory to run in");
                throw new ArgumentException("This utility expects 2 arguments. " +
                    "The type of process to run and the path to the folder to " +
                    "check/update license headers.");
            }
            var commandArg = args[0];
            Command cmd = Command.Report;
            if(commandArg == "-e")
            {
                cmd = Command.Enforce;
            }

            var path = args[1];
            var dir = new DirectoryInfo(path);
            if(dir.Exists == false)
            {
                throw new ArgumentException($"Directory '{path}' does not exist.");
            }
            var config = ReadConfig();

            switch (cmd)
            {
                case Command.Report:
                    LicenseHeaderProcess(dir, config, false);
                    break;
                case Command.Enforce:
                    LicenseHeaderProcess(dir, config, true);
                    break;
                default:
                    break;
            }


            Console.WriteLine("Done");
            Console.ReadKey();
        }

        static bool InDir(DirectoryInfo dir, string dirName)
        {
            return dir.Name.Equals(dirName, StringComparison.OrdinalIgnoreCase) ||
                (dir.Parent != null && InDir(dir.Parent, dirName));
        }

        static DirectoryConfig GetDirConfig(DirectoryInfo dir, Config config)
        {
            var result = config.directoryConfigs.Where(c => dir.Name.Contains(c.directoryKey));

            if (result.Count() == 0)
            {
                throw new Exception($"Could not find any configurations for directory '{dir.Name}'. " +
                    $"Directory must contain one of the following substrings: " +
                    $"{string.Join(",", config.directoryConfigs.Select(c => c.directoryKey))}");    
            }
            else if(result.Count() > 1)
            {
                throw new Exception($"Multiple configurations matched directory '{dir.Name}'. " +
                    $"Matching keys were: {string.Join(",", result.Select(r => r.directoryKey))}");
            }

            Console.WriteLine($"Using configuration for {result.Single().directoryKey}");

            return result.Single();
        }

        private static List<string> GetSubmoduleDirs(DirectoryInfo dir)
        {
            List<string> result = new List<string>();
            var gitmodules = dir.GetFiles(".gitmodules");
            if(gitmodules.Count() > 0)
            {
                foreach(var line in File.ReadAllLines(gitmodules.Single().FullName))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("path"))
                    {
                        result.Add(trimmed.Substring(
                            trimmed.LastIndexOfAny(new char[] { '=', '/' }) + 1).Trim());
                    }
                }
            }

            return result;
        }

        private static bool IsCopyrightStart(char[] buffer, int k, bool isPythodDir)
        {
            if (k < 0 || k + 1 >= buffer.Length)
            {
                return false;
            }
            // Comment blocks will start with either
            // - /* (Most languages)
            // - @* (c# razor syntax)
            // - # (Python), except shebang (#! at file start)
            return ((buffer[k] == '/' || buffer[k] == '@') && buffer[k + 1] == '*')
            || (isPythodDir && buffer[k] == '#' && !(k == 0 && buffer[k + 1] == '!'));
        }

        private static bool IsCopyrightBreak(char[] buffer, int k, bool isPythodDir)
        {
            if (k < 0 || k + 2 >= buffer.Length)
            {
                return false;
            }
            // Comment blocks will end with
            // - */ (Most languages)
            // - *@ (c# razor syntax)
            // - first new line that does not start with a # (Python)
            // - first new line that starts with a ## (Python)
            return (buffer[k] == '*'
                    && (buffer[k + 1] == '/' || buffer[k + 1] == '@'))
            || (isPythodDir
                && buffer[k] == '\n'
                && (!((buffer[k + 1] == '#') || (buffer[k + 2] == '#'))
                    || ((buffer[k + 1] == '#') && (buffer[k + 2] == '#'))
                    || IsCopyrightBreak(buffer, k + 1, isPythodDir)));
        }

        private static void LicenseHeaderProcess(DirectoryInfo dir, Config config, bool enforce)
        {
            var dirConfig = GetDirConfig(dir, config);
            var submoduleDirs = GetSubmoduleDirs(dir);
            var dirsToIgnore = submoduleDirs.Concat(config.dirsToIgnore);

            var filteredFiles = GetFiles(dir, dirsToIgnore, dirConfig, config);

            int matching = 0;
            int updated = 0;
            int added = 0;
            int unknownpatent = 0;

            var filesByLicenseText = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            bool isPythodDir = dirConfig.directoryKey == "python";

            Parallel.ForEach(filteredFiles, file =>
            {
                try
                {
                    StringBuilder message = new StringBuilder(file.FullName);

                    string commentText = "";

                    using (var reader = file.OpenText())
                    {
                        char[] buffer = new char[file.Length];

                        reader.ReadBlock(buffer, 0, buffer.Length);

                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (IsCopyrightStart(buffer, i, isPythodDir))
                            {
                                for (int j = i + 1; j < buffer.Length; j++)
                                {
                                    if (IsCopyrightBreak(buffer, j, isPythodDir))
                                    {
                                        // If we're dealing with a Python file then make sure we
                                        // retain the final 'newline'.
                                        if (dirConfig.directoryKey == "python")
                                        {
                                            j -= 2;
                                        }
                                        commentText = new string(buffer.Skip(i).Take(j - i + 2).ToArray());
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    if (commentText.Contains("copyright", StringComparison.OrdinalIgnoreCase))
                    {
                        var newComment = "";
                        if (commentText.Contains("patent", StringComparison.OrdinalIgnoreCase))
                        {
                            if (commentText.Contains("2871816"))
                            {
                                newComment = config.patternLicenseText;
                            }
                            else if (commentText.Contains("3438848"))
                            {
                                newComment = config.hashLicenseText;
                            }
                            else
                            {
                                Interlocked.Increment(ref unknownpatent);
                            }
                        }
                        else
                        {
                            newComment = GetCorrectComment(config, file.FullName, dirConfig.directoryKey);
                        }

                        if (enforce)
                        {
                            if (commentText == newComment)
                            {
                                Interlocked.Increment(ref matching);
                            }
                            else
                            {
                                // We have an existing license comment but it 
                                // doesn't match so replace it with the correct one.
                                var temp = Path.GetTempFileName();
                                File.WriteAllText(temp, File.ReadAllText(file.FullName).Replace(commentText, newComment));
                                File.Replace(temp, file.FullName, null);
                                Interlocked.Increment(ref updated);
                            }
                        }
                    }
                    // No existing license comment so add one.
                    else if (commentText.Contains("swig", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        if (enforce)
                        {
                            Interlocked.Increment(ref added);
                            var temp = Path.GetTempFileName();

                            if (file.Extension == ".php")
                            {
                                File.WriteAllText(temp, "<?php" + "\n" + config.licenseText + "\n\n" + File.ReadAllText(file.FullName).Replace("<?php", ""));
                            }
                            else
                            {
                                var newComment = GetCorrectComment(config, file.FullName, dirConfig.directoryKey);
                                File.WriteAllText(temp, newComment + "\n\n" + File.ReadAllText(file.FullName));
                            }
                            File.Replace(temp, file.FullName, null);
                        } 
                    }

                    if(enforce == false &&
                        commentText.Contains("copyright", StringComparison.OrdinalIgnoreCase) &&
                        commentText.Contains("swig", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        var bag = filesByLicenseText.GetOrAdd(commentText,
                            new ConcurrentBag<string>());
                        bag.Add(file.FullName);
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Error with file '{file.FullName}'" +
                        $"\n{ex.GetType()} - {ex.Message}";
                    Console.WriteLine(message);
                    throw;
                }
            });


            if (enforce)
            {
                Console.WriteLine($"Total files checked: {filteredFiles.Count()}");
                Console.WriteLine($"License comment updated: {updated}");
                Console.WriteLine($"License comment added: {added}");
                Console.WriteLine($"License comment verified: {matching}");

                if (unknownpatent > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"License comment for unknown patent: {unknownpatent}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else
            {
                if (filesByLicenseText.ContainsKey(""))
                {
                    Console.WriteLine($"Files with no comment:");
                    foreach (var file in filesByLicenseText[""])
                    {
                        Console.WriteLine(file);
                    }
                    Console.WriteLine();
                }

                foreach (var text in filesByLicenseText.Where(kvp => kvp.Key.Length > 0)
                    .OrderByDescending(c => c.Value.Count))
                {
                    Console.WriteLine($"{text.Value.Count} instances of comment:");
                    foreach (var file in text.Value)
                    {
                        Console.WriteLine(file);
                    }
                    Console.WriteLine(text.Key);
                    Console.WriteLine();
                }
            }
        }

        private static string GetCorrectComment(Config config, string filename, string directoryKey)
        {
            var newComment = config.licenseText;
            if (filename.EndsWith(".cshtml"))
            {
                newComment = newComment.Replace("/*", "@*");
                newComment = newComment.Replace("*/", "*@");
            }
            if (directoryKey == "python")
            {
                newComment = newComment.Replace("/*", "#");
                newComment = newComment.Replace(" */", "");
                newComment = newComment.Replace("\n *", "\n#");
            }
            return newComment;
        }

        private static IEnumerable<FileInfo> GetFiles(DirectoryInfo dir, 
            IEnumerable<string> dirsToIgnore, DirectoryConfig dirConfig, Config config)
        {
            var files = dir.EnumerateFiles("*", SearchOption.AllDirectories);
            return files.Where(f => dirsToIgnore.All(d => InDir(f.Directory, d) == false) &&
                    dirConfig.extensionsToUpdate.Any(e => f.FullName.EndsWith(e, StringComparison.OrdinalIgnoreCase)) &&
                    config.extensionsToIgnore.Any(e => f.FullName.EndsWith(e, StringComparison.OrdinalIgnoreCase)) == false);
        }

    }
}
