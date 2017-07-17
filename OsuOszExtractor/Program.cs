using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuOszExtractor
{
    class Program
    {
        private const string SONGS_DIR = "songs";
        static void Main(string[] args)
        {
            Console.WriteLine("Finding Songs folder...");
            string currentDir = Directory.GetCurrentDirectory();

            string songsDir = currentDir + "\\" + SONGS_DIR;
            if (!Directory.Exists(songsDir))
            {
                Console.WriteLine("Songs folder could not be found.");
                Environment.Exit(1);
            }
            Console.WriteLine("Songs folder found");
            string[] files = null;
            try
            {
                files = Directory.GetFiles(songsDir);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
            //Extract all zip files
            foreach (var path in files)
            {
                var extension = Path.GetExtension(path);
                if (extension != ".osz")
                {
                    continue;
                }
                Path.GetExtension(path);
                Console.ResetColor();
                string fileName = Path.GetFileNameWithoutExtension(path);
                string outputPath = songsDir + "\\" + fileName;
                try
                {
                    Console.Write("Extracting " +fileName + "...");
                    OsuOszExtrator.ExtractOsz(path,outputPath);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("done\n");
                }                
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("failed\n");
                    Console.ResetColor();
                    Console.WriteLine(e.Message);

                }
                
            }
        }
        
    }
}
