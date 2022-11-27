using System.Security.Cryptography;

namespace DuplicateFilesScannerAndDelete
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path;
            ConsoleKeyInfo cki;
            double totalSize = 0;
            //pass directory path as argument to command line
            if (args.Length > 0)
                path = args[0] as string;
            else
                path = @"C:\Users\Madhusudhan\Downloads\Test1";

            Console.WriteLine("Scanning in path(includes nested):"+path);
            //Get all files from given directory
            var fileLists = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).OrderBy(p => p).ToList();
            //var fileLists = Directory.GetFileSystemEntries(path);
            int totalFiles = fileLists.Count();
            Console.WriteLine("totalFiles found:"+totalFiles);
            List<FileDetails> finalDetails = new List<FileDetails>();
            List<string> ToDelete = new List<string>();
            finalDetails.Clear();

            Console.WriteLine("Calculating hash of each files and generating the list for comparison");
            //loop through all the files by file hash code
            foreach (var item in fileLists)
            {
                using (var fs = new FileStream(item, FileMode.Open, FileAccess.Read))
                {
                    finalDetails.Add(new FileDetails()
                    {
                        FileName = item,
                        FileHash = BitConverter.ToString(SHA1.Create().ComputeHash(fs)),
                    });
                }
            }
            //group by file hash code
            var similarList = finalDetails.GroupBy(f => f.FileHash)
                .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });


            //keeping first item of each group as is and identify rest as duplicate files to delete
            ToDelete.AddRange(similarList.SelectMany(f => f.Files.Skip(1)).ToList());
            Console.WriteLine("Total duplicate files - {0}", ToDelete.Count);
            //list all files to be deleted and count total disk space to be empty after delete
            if (ToDelete.Count > 0)
            {
                Console.WriteLine("Files to be deleted - ");
                foreach (var item in ToDelete)
                {
                    Console.WriteLine(item);
                    FileInfo fi = new FileInfo(item);
                    totalSize += fi.Length;
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Total space free up by -  {0}mb", Math.Round((totalSize / 1000000), 6).ToString());
            Console.ForegroundColor = ConsoleColor.White;
            //delete duplicate files
            if (ToDelete.Count > 0)
            {
                Console.WriteLine("Press C to continue with delete");
                Console.WriteLine("Press the Escape (Esc) key to quit: \n");
                do
                {
                    cki = Console.ReadKey();
                    Console.WriteLine(" --- You pressed {0}\n", cki.Key.ToString());
                    if (cki.Key == ConsoleKey.C)
                    {
                        Console.WriteLine("Deleting files...");
                        ToDelete.ForEach(File.Delete);
                        Console.WriteLine("Files are deleted successfully");
                    }
                    Console.WriteLine("Press the Escape (Esc) key to quit: \n");
                } while (cki.Key != ConsoleKey.Escape);
            }
            else
            {
                Console.WriteLine("No files to delete");
                Console.ReadLine();
            }
        }
    }
    public class FileDetails
    {
        public string FileName { get; set; }
        public string FileHash { get; set; }
    }
}