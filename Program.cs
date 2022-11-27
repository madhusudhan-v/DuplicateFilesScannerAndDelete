using System.Security.Cryptography;

namespace DuplicateFilesScannerAndDelete
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "";

            ConsoleKeyInfo cki;
            double totalSize = 0;
            //pass directory path as argument to command line
            if (args.Length > 0)
                path = args[0] as string;
            else
            {
                //path = @"C:\Madhus Photos\Camera";
                Console.WriteLine("Please provide valid files path(folder path) and then enter eg:C:\\Madhus Photos\\1Captured\\camera1 videos");
                do
                {
                    path = Console.ReadLine() ?? "";
                    //place for user input   
                } while (!DirectoryAccessible(path));
            }
            var logResultFile = Path.Combine(path, "DuplicateFilesScannerAndDelete.txt");
            Console.WriteLine("Scanning in path(includes nested):" );
            //Get all files from given directory
            var fileLists = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).OrderBy(p => p).ToList();
            fileLists.Sort();
            fileLists.Remove(logResultFile);

            Console.WriteLine("\n Total Files found in directory are(includes nested):" + fileLists.Count());
            var typesGroup = fileLists.GroupBy(x => x.Substring(x.LastIndexOf(".")));
            foreach (var t in typesGroup)
            {
                Console.WriteLine(t.Count() + " " + t.Key + " file(s)");
            }

            List<string> ToDelete = new List<string>();
            Console.WriteLine("\nChoose Mode type to scan and deletion");
            Console.WriteLine("S:Slow mode all files compare by hash (More perfect)");
            Console.WriteLine("F:Fast mode 2 step activity(1:Scan all files by size then 2:Then for same size files hash comparison)");
            Console.WriteLine("B:Both process and select based on result(Very slow because both process had to complete)");
            Console.WriteLine("Press the Escape (Esc) key to quit: \n");
            List<FileDetails> finalDetails = new List<FileDetails>();
            bool wait = true;
            do
            {
                cki = Console.ReadKey();
                Console.WriteLine(" --- You pressed {0}\n", cki.Key.ToString());
                if (cki.Key == ConsoleKey.S || cki.Key == ConsoleKey.F || cki.Key == ConsoleKey.Escape || cki.Key == ConsoleKey.B)
                {
                    if (cki.Key == ConsoleKey.Escape)
                        return;
                    else if (cki.Key == ConsoleKey.S)
                        ToDelete = CompareSlowlyAllByHashValues(fileLists);
                    else if (cki.Key == ConsoleKey.F)
                        ToDelete = CompareFastFirstBySizeThenByHash(fileLists);
                    else if (cki.Key == ConsoleKey.B)
                    {
                        var finalDetailsFast = CompareFastFirstBySizeThenByHash(fileLists);
                        Console.WriteLine("\n");
                        var finalDetailsSlow = CompareSlowlyAllByHashValues(fileLists);
                        Console.WriteLine("\n");
                        if (!finalDetailsSlow.Any() && !finalDetailsFast.Any())
                            goto ContinueToCleanFolder;
                        Console.WriteLine("Choose Clean files mode type");
                        Console.WriteLine("S:Slow scan found:" + finalDetailsSlow.Count());
                        Console.WriteLine("F:Fast scan found:" + finalDetailsFast.Count());
                        Console.WriteLine("Press the Escape (Esc) key to quit: \n");
                        bool wait2 = true;
                        do
                        {
                            cki = Console.ReadKey();
                            Console.WriteLine(" --- You pressed {0}\n", cki.Key.ToString());
                            if (cki.Key == ConsoleKey.S || cki.Key == ConsoleKey.F || cki.Key == ConsoleKey.Escape)
                            {
                                if (cki.Key == ConsoleKey.S)
                                    ToDelete = finalDetailsSlow;
                                else if (cki.Key == ConsoleKey.F)
                                    ToDelete = finalDetailsFast;
                                wait2 = false;
                            }
                        } while (wait2);
                    }
                    wait = false;
                }
            } while (wait);

            //group by file hash code
            //var similarList = finalDetails.GroupBy(f => f.FileHash)
            //    .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });

            ////keeping first item of each group as is and identify rest as duplicate files to delete
            //ToDelete.AddRange(similarList.SelectMany(f => f.Files.Skip(1)).ToList());
            Console.WriteLine("Total duplicate files by hash - {0}", ToDelete.Count);
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
                        using (StreamWriter sw = File.Exists(logResultFile) ? File.AppendText(logResultFile) : new StreamWriter(logResultFile))
                        {
                            sw.WriteLineAsync("\n");
                            var startTime = DateTime.Now;
                            sw.WriteLineAsync("Execution on " + DateTime.Now);
                            Console.WriteLine("Deleting files...");
                            sw.WriteLineAsync("Deleting files...");
                            ToDelete.ForEach(x =>
                            {
                                File.Delete(x);
                                sw.WriteLineAsync(x);
                            });
                            Console.WriteLine($"{ToDelete.Count()} Files deleted successfully({(DateTime.Now - startTime).TotalMilliseconds} milliseconds) at:" + DateTime.Now);
                            sw.WriteLineAsync($"{ToDelete.Count()} Files deleted successfully({(DateTime.Now - startTime).TotalMilliseconds} milliseconds) at:" + DateTime.Now);
                            sw.Close();
                            Console.WriteLine("Press escape to proceed next");
                        }
                    }
                } while (cki.Key != ConsoleKey.Escape);
                //CleanEmptyFolders(path);
            }
            else
            {
                Console.WriteLine("No files to delete");

            }
        ContinueToCleanFolder:
            Console.WriteLine("Press D to clean empty folders");
            Console.WriteLine("Press the Escape (Esc) key to quit: \n");
            do
            {
                cki = Console.ReadKey();
                Console.WriteLine(" --- You pressed {0}\n", cki.Key.ToString());
                if (cki.Key == ConsoleKey.D)
                {
                    Console.WriteLine("Cleaning empty subdirectories");
                    CleanEmptyFolders(path);
                    Console.WriteLine("Empty folders deleted successfully");
                }
                Console.WriteLine("Press the Escape (Esc) key to quit: \n");
            } while (cki.Key != ConsoleKey.Escape);
            //Console.ReadLine();
        }
        private static List<string> CompareFastFirstBySizeThenByHash(List<string> fileLists)
        {
            Console.WriteLine("FAST MODE");
            List<FileDetails> firstLevelFinalDetails = new List<FileDetails>();

            firstLevelFinalDetails.Clear();

            Console.WriteLine("2 step scanning(Size compare then Hash compare)");

            Console.WriteLine("Scanning files by size");
            var startTime = DateTime.Now;
            //1st level scanning
            //loop through all the files by file hash code
            using (var progress = new ProgressBar())
            {
                foreach (var item in fileLists)
                {
                    progress.Report((double)fileLists.IndexOf(item) / fileLists.Count());
                    using (var fs = new FileStream(item, FileMode.Open, FileAccess.Read))
                    {
                        firstLevelFinalDetails.Add(new FileDetails(item, fs.Length));
                    }
                }
            }
            Console.WriteLine("Total time consumed(in milliseconds) Reading by length (1st level scanning):" + (DateTime.Now - startTime).TotalMilliseconds);

            Console.WriteLine("Calculating hash of each files and generating the list for comparison");
            //group by file hash code
            var similarListInSizeLength = firstLevelFinalDetails.GroupBy(f => f.Length)
                .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });
            var duplicatesGroupByLengthIncludingOrignial = similarListInSizeLength.Where(x => x.Files.Count() > 1);//here ignoring single files
            int totalDuplicates = 0;
            foreach (var item in duplicatesGroupByLengthIncludingOrignial)
            {
                totalDuplicates += item.Files.Count();
            }
            int totalDuplicateFilesIncludingOrignial = totalDuplicates;
            totalDuplicates = totalDuplicates - duplicatesGroupByLengthIncludingOrignial.Count();
            Console.WriteLine("Found duplicate files by size:" + totalDuplicates);
            List<string> ToDelete = new List<string>();
            if (totalDuplicates == 0)
                return ToDelete;
            //Second level scanning
            List<FileDetails> f1 = new List<FileDetails>();
            startTime = DateTime.Now;
            var counter = 0;
            using (var progress = new ProgressBar())
            {
                foreach (var item in duplicatesGroupByLengthIncludingOrignial)
                {
                    foreach (var i in item.Files)
                    {
                        progress.Report((double)++counter / totalDuplicateFilesIncludingOrignial);
                        using (var fs = new FileStream(i, FileMode.Open, FileAccess.Read))
                        {
                            f1.Add(new FileDetails(i, BitConverter.ToString(SHA1.Create().ComputeHash(fs))));
                        }
                    }

                }
            }
            Console.WriteLine("Total time consumed(in milliseconds) Reading by Hash (2nd level scanning):" + (DateTime.Now - startTime).TotalMilliseconds);

            var similarList = f1.GroupBy(f => f.FileHash)
               .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });

            //keeping first item of each group as is and identify rest as duplicate files to delete

            ToDelete.AddRange(similarList.SelectMany(f => f.Files.Skip(1)).ToList());
            Console.WriteLine("Found duplicate files by hash:" + ToDelete.Count());
            return ToDelete;
        }
        private static List<string> CompareSlowlyAllByHashValues(List<string> fileLists)
        {
            Console.WriteLine("SLOW MODE");
            List<FileDetails> finalDetails = new List<FileDetails>();

            finalDetails.Clear();

            Console.WriteLine("Calculating hash of each files and generating the list for comparison(All files hash comparison)");

            var startTime = DateTime.Now;
            //loop through all the files by file hash code
            using (var progress = new ProgressBar())
            {
                foreach (var item in fileLists)
                {
                    progress.Report((double)fileLists.IndexOf(item) / fileLists.Count());
                    using (var fs = new FileStream(item, FileMode.Open, FileAccess.Read))
                    {
                        finalDetails.Add(new FileDetails(item, BitConverter.ToString(SHA1.Create().ComputeHash(fs))));
                    }
                }
            }
            Console.WriteLine("Total time consumed(in milliseconds):" + (DateTime.Now - startTime).TotalMilliseconds);
            /* //Using file length is not accurate, its leads wrong... not perfect
           startTime = DateTime.Now;
           List<FileDetails> finalDetails2 = new List<FileDetails>();
           foreach (var item in fileLists)
           {
               using (var fs = new FileStream(item, FileMode.Open, FileAccess.Read))
               {
                   finalDetails2.Add(new FileDetails()
                   {
                       FileName = item,
                       Length = item.Length,
                   });
               }
           }
           Console.WriteLine("Total time consumed(2):" + (DateTime.Now - startTime).TotalMilliseconds);
           var similarList2 = finalDetails2.GroupBy(f => f.Length)
               .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });
           List<string> ToDelete2 = new List<string>();
           ToDelete2.AddRange(similarList2.SelectMany(f => f.Files.Skip(1)).ToList());
           Console.WriteLine("Total duplicate files(2) - {0}", ToDelete2.Count);
           //Using file length is not accurate, its leads wrong... not perfect
           */
            var similarList = finalDetails.GroupBy(f => f.FileHash)
              .Select(g => new { FileHash = g.Key, Files = g.Select(z => z.FileName).ToList() });

            List<string> ToDelete = new List<string>();
            //keeping first item of each group as is and identify rest as duplicate files to delete
            ToDelete.AddRange(similarList.SelectMany(f => f.Files.Skip(1)).ToList());
            Console.WriteLine("Found duplicate files by hash:" + ToDelete.Count());
            return ToDelete;
        }
        private static void CleanEmptyFolders(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                CleanEmptyFolders(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
        public static bool DirectoryAccessible(string path)
        {
            try
            {
                var exist = Directory.Exists(path);
                if (exist)
                    Console.WriteLine("Valid Choosen path : \"" + path.ToUpper() + "\"");
                else
                    Console.WriteLine("Invalid path :\"" + path.ToUpper() + "\" \n Please provide proper path");
                return exist;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("You dont have permission to access this path");
                return false;
            }
            catch
            {
                Console.WriteLine("Invalid path");
                return false;
            }
        }
    }
    public class FileDetails
    {
        public FileDetails(string fileName, string fileHash)
        {
            FileName = fileName;
            FileHash = fileHash;
        }
        public FileDetails(string fileName, long length)
        {
            FileName = fileName;
            Length = length;
            FileHash = string.Empty;
        }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public long Length { get; set; }
    }
}