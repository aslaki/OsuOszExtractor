using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace OsuOszExtractor
{
    public class OsuOszExtrator
    {
        private const string OSU_FILE_EXTENSION = ".osu";

        public static void ExtractOsz(string path, string outputDir)
        {
            if (!IsOsuFile(path))
            {
                throw new NotOsuFileException("File '" + path + "' is not an osu file.");
            }

            // Parallel Extraction:
            _ = Parallel.ForEach(ZipFile.OpenRead(path).Entries,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                entry =>
                {
                    // Ensure output directory exists:
                    _ = Directory.CreateDirectory(outputDir);
                    entry.ExtractToFile(Path.Combine(outputDir, entry.FullName), true); // Overwrite existing files
                });
        }

        public static bool IsOsuFile(string path)
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                return archive.Entries
                    .Where(entry => entry.Name.EndsWith(OSU_FILE_EXTENSION))
                    .Any(); // Check if there's at least one .osu file
            }
        }

        public class NotOsuFileException : Exception
        {
            public NotOsuFileException() : base() { }
            public NotOsuFileException(string msg) : base(msg) { }
        }
    }
}