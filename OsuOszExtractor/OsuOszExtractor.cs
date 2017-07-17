using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuOszExtractor
{
    public class OsuOszExtrator
    {
        private const string OSU_FILE_EXTENSION = ".osu";
        public static void ExtractOsz(string path,string outputDir)
        {
            if (IsOsuFile(path))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(path, outputDir);
            }
            else
            {
                throw new NotOsuFileException("File '"+path+"' is not osu file");
            }
            
        }

        public static bool IsOsuFile(string path)
        {
            FileStream fileStream = new FileStream(path,FileMode.Open);
            using (ZipArchive archive = new ZipArchive(fileStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(OSU_FILE_EXTENSION))
                    {
                        return true;
                    }
                    
                }
            }
            return false;
        }
        public class NotOsuFileException : Exception
        {
            public NotOsuFileException() : base() { }
            public NotOsuFileException(string msg) : base(msg) { }
        }
    }
}
