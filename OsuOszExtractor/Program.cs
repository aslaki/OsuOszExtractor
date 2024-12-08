using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq; // Added for LINQ queries
using System.Threading.Tasks;

namespace OsuOszExtractor
{
    internal class Program
    {
        private const string SONGS_DIR = "songs";

        private static void Main()
        {
            Console.WriteLine("Finding Songs folder...");

            // Simplified path combination
            string songsDir = Path.Combine(Directory.GetCurrentDirectory(), SONGS_DIR);

            if (!Directory.Exists(songsDir))
            {
                Console.WriteLine("Songs folder could not be found. Expected location: {0}", songsDir);
                return;
            }

            Console.WriteLine("Songs folder found");

            string[] oszFiles = Directory.GetFiles(songsDir).Where(f => Path.GetExtension(f) == ".osz").ToArray();

            int maxDegreeOfParallelismOuterLoop = Environment.ProcessorCount;
            int maxDegreeOfParallelismInnerLoop = (int)Math.Ceiling(Environment.ProcessorCount * 0.75);

            Dictionary<string, ZipArchive> openedArchives = new Dictionary<string, ZipArchive>();
            _ = Parallel.ForEach(oszFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelismOuterLoop }, path =>
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                try
                {
                    Console.Write($"Opening {fileName}...");
                    ZipArchive archive = ZipFile.OpenRead(path);
                    openedArchives.Add(path, archive);  // Store archive with its path as the key
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" done\n");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error opening {fileName}: {ex.Message}");
                    Console.ResetColor();
                }
            });

            // Optimized Parallel Extraction using ZipFile
            _ = Parallel.ForEach(openedArchives, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelismInnerLoop }, kvp =>
            {
                string path = kvp.Key;
                ZipArchive archive = kvp.Value;

                string fileName = Path.GetFileNameWithoutExtension(path);
                string outputPath = Path.Combine(songsDir, fileName);

                try
                {
                    // Check if there's a song folder in the archive
                    ZipArchiveEntry songFolderEntry = archive.Entries.FirstOrDefault(e => e.FullName.StartsWith(fileName + "/") && e.FullName.EndsWith("/"));

                    if (songFolderEntry != null)
                    {
                        // Extract contents of the song folder in parallel
                        _ = Parallel.ForEach(archive.Entries.Where(e => e.FullName.StartsWith(songFolderEntry.FullName)), entry =>
                        {
                            string entryOutputPath = Path.Combine(outputPath, entry.FullName.Substring(songFolderEntry.FullName.Length));
                            string longPath = @"\\?\" + entryOutputPath; // Use \\?\ prefix to avoid limits
                            _ = Directory.CreateDirectory(Path.GetDirectoryName(longPath));
                            entry.ExtractToFile(longPath, true);
                        });
                    }
                    else
                    {
                        // Extract all files directly if there's no song folder
                        string longOutputPath = @"\\?\" + outputPath;
                        archive.ExtractToDirectory(longOutputPath);
                    }
                }
                catch (OsuOszExtrator.NotOsuFileException)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow; // Warning color
                    Console.WriteLine($"Warning: Skipping {fileName} - not a valid .osz file.");
                    Console.ResetColor();
                }
                catch (AggregateException aggregateEx)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    foreach (Exception innerEx in aggregateEx.InnerExceptions)
                    {
                        Console.WriteLine($"Error extracting {fileName}: {innerEx.GetType().Name}: {innerEx.Message}");

                        // Check if the exception provides file or path information through its TargetSite property
                        if (innerEx.TargetSite != null && innerEx.TargetSite.DeclaringType != null)
                        {
                            string targetType = innerEx.TargetSite.DeclaringType.FullName;
                            string targetMethod = innerEx.TargetSite.Name;

                            // Log information if available
                            if (targetType == "System.IO.FileStream")
                            {
                                Console.WriteLine($"File: {innerEx.TargetSite.DeclaringType.GetProperty("Name")?.GetValue(innerEx.TargetSite.DeclaringType)}");
                                Console.WriteLine($"Path: {innerEx.TargetSite.DeclaringType.GetProperty("Path")?.GetValue(innerEx.TargetSite.DeclaringType)}");
                            }
                            else
                            {
                                Console.WriteLine($"Target: {targetType}.{targetMethod}");
                            }
                        }

                        Console.WriteLine(innerEx.StackTrace);
                    }

                    Console.ResetColor();
                }
                catch (IOException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow; // for warnings
                    Console.WriteLine($"Warning: Error extracting {fileName}: {ex.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex) // Catch any other unexpected exceptions. Not that the user will be able to see them with how fast this is running, but anyway.
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error extracting {fileName}: {ex.GetType().Name}: {ex.Message}");
                    Console.ResetColor();
                }
                finally
                {
                    archive.Dispose();
                }
            });
            Console.WriteLine("Extraction process complete.");
        }
    }
}