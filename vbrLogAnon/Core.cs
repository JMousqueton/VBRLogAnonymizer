using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Concurrent;

namespace vbrLogAnon
{
    internal class Core
    {
        private static Hashtable _anonymizationDictionnary = new Hashtable();
     
        public static void ApplyAllReplacements(List<ReplacementItem> replacementsItems, ref string lines) {           
            foreach(ReplacementItem repItem in replacementsItems)
            {
                repItem.ApplyReplacement(ref lines, ref _anonymizationDictionnary);
            }
        }


        public static void AnonymizeFile(string sourceFileName,string destinationFileName,Configuration conf) {
            if (File.Exists(sourceFileName))
            {
                FileInfo inputFileInfo = new FileInfo(sourceFileName);
                string fileContent = "";
                if (inputFileInfo.Length / 1048576 <= conf.MaxFileInMemoryMB)
                {
                    StreamReader inputFile = new StreamReader(sourceFileName);
                    try
                    {
                        fileContent = inputFile.ReadToEnd();
                        Core.ApplyAllReplacements(conf.ReplacementItems, ref fileContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while parsing: " + inputFileInfo.FullName + ". Error was: " + ex.Message);
                        Console.WriteLine("Aborting now. Exiting");
                        Environment.Exit(1);
                    }
                    finally {
                        inputFile.Dispose();
                    }

                    StreamWriter outputFile = new StreamWriter(destinationFileName);
                    try
                    {
                        outputFile.Write(fileContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while writing to file: " + destinationFileName + ". Error was: " + ex.Message);
                        Environment.Exit(1);
                    }
                    finally
                    {
                        outputFile.Dispose();
                        fileContent = "";
                    }

                }
                else {
                    StreamReader inputFile = new StreamReader(sourceFileName);
                    StreamWriter outputFile = new StreamWriter(destinationFileName);
                    string lineGroup = "";
                    try
                    {
                        int numLine = 0;
                        
                        while (!inputFile.EndOfStream)
                        {
                            numLine++;
                            lineGroup +=  inputFile.ReadLine()+ System.Environment.NewLine;
                            if (numLine == 20) {
                                numLine = 0;
                                Core.ApplyAllReplacements(conf.ReplacementItems, ref lineGroup);
                                outputFile.Write(lineGroup);
                                lineGroup = "";
                            }
                        }
                        Core.ApplyAllReplacements(conf.ReplacementItems, ref lineGroup);
                        outputFile.Write(lineGroup);
                        lineGroup = "";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while parsing: " + inputFileInfo.FullName + ". Error was: " + ex.Message);
                        Environment.Exit(1);
                    }
                    finally
                    {
                        inputFile.Dispose();
                        outputFile.Dispose();
                        lineGroup = "";
                    }

                }
           
            }
        }

        public static void DoWork(bool multiCore, string sourceDir, string destDir, Configuration conf) {
            int numFiles = 0;
            if (Directory.Exists(sourceDir)) {
                Console.WriteLine("Scanning source directory structure and building target directory structure");
                if (File.Exists(sourceDir + @"\anonDictionnary.txt")) {
                    try
                    {
                        File.Delete(sourceDir + @"\anonDictionnary.txt");
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Could not delete " + sourceDir + @"\anonDictionnary.txt. Error was: " + ex.Message);
                    }
                }
                var watch = System.Diagnostics.Stopwatch.StartNew();

                foreach (string dir in Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories)) {
                    try
                    {
                        Directory.CreateDirectory(dir.Replace(sourceDir, destDir));
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Error creating target directory sructure. Error was: " + ex.Message);
                        Environment.Exit(1);
                    }               
                }
                Console.WriteLine("Target directory structure built.");
                Console.WriteLine("Parsing files");
                Console.Write(".");
                string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                if (files.Length < 1) {
                    Console.WriteLine("No files found in source directory. Aborting");
                    Environment.Exit(0);
                }
                if (multiCore)
                {
                    Parallel.ForEach(files, file => {
                        Core.AnonymizeFile(file, file.Replace(sourceDir, destDir), conf);
                        numFiles++;
                        Console.Write(".");
                    });          
                }else {
                    foreach (string file in files) {
                        Core.AnonymizeFile(file, file.Replace(sourceDir, destDir), conf);
                        numFiles++;
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
                watch.Stop();
                Console.WriteLine("Done processing " + numFiles + " files in " + watch.ElapsedMilliseconds+"ms") ;
                Core.WriteDictionnary(sourceDir);
                Console.Write("Exiting now. Goodbye.");
            }
            else
            {
                Console.WriteLine("Source directory does not exists.");
                Environment.Exit(1);
            }
        }

        public static void ShowUsage() {
            Console.WriteLine("Usage :");
            Console.WriteLine("-s <source dir> : absolute path to source dir containing Veeam logs.");
            Console.WriteLine("-d <destination dir> : absolute path to destination dir where anonymized logs will be written. Must exists beforehand.");
            Console.WriteLine("[-c <config file>] : absolute path to Json configuration file. If not present, will use config.json in the same dir as this executable. ");
            Console.WriteLine();
            Environment.Exit(0);
        
        }
        public static void WriteDictionnary(string sourceDir) {
            
            try
            {
                StreamWriter dicFile = new StreamWriter(sourceDir + @"\anonDictionnary.txt",false);
                foreach (AnonymizationItem item in _anonymizationDictionnary.Values)
                {
                    dicFile.WriteLine(item.ReplacementItemOriginalText + "," + item.ReplacementItemNewText);
                }
                Console.WriteLine("Dictionnary written at " + sourceDir + @"\anonDictionnary.txt");
                dicFile.Close();
                dicFile.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing dictionnary file. Error was: " + ex.Message);
            }


        }
        internal class ReplacementItem
        {
            private string _name = "";
            private string _replacementRegex = "";
            private string _replacementText = "";
            private bool _directoryStructure = false;
            private bool _useRandom = true;
            private int _randomLength = 5;
            private string _replacementString = "*";

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
            public string ReplacementRegex
            {
                get { return _replacementRegex; }
                set { _replacementRegex = value; }
            }
            public string ReplacementText
            {
                get { return _replacementText; }
                set { _replacementText = value; }
            }

            public bool DirectoryStructure
            {
                get { return _directoryStructure; }
                set { _directoryStructure = value; }
            }
            public bool UseRandom
            {
                get { return _useRandom; }
                set { _useRandom = value; }
            }

            public string ReplacementString
            {
                get { return _replacementString; }
                set { _replacementString = value; }
            }

            public int RandomLength
            {
                get { return _randomLength; }
                set { _randomLength = value; }
            }

            public void ApplyReplacement(ref string lines, ref Hashtable anoDictionnary)
            {
                try
                {
                    Match m = Regex.Match(lines, _replacementRegex);
                    if (m.Success)
                    {
                        Core.AnonymizationItem anoItem = new Core.AnonymizationItem();
                        lock (anoDictionnary)
                        {
                            if (anoDictionnary.ContainsKey(m.Value))
                            {
                                anoItem = (Core.AnonymizationItem)anoDictionnary[m.Value];
                            }
                            else
                            {
                                 anoItem = Core.AnonymizationItem.buildNew(this, m.Value);
                                 anoDictionnary.Add(m.Value, anoItem); 
                            }
                        }
                        //solution1
                        lines =Regex.Replace(lines, _replacementRegex, anoItem.ReplacementItemNewText);
                        //solution2
                        //lines = lines.Replace(m.Value, anoItem.ReplacementItemNewText);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while applying pattern for " + _name + ". Error was: " + ex.Message);
                }

            }
        }
        internal class AnonymizationItem
        {
            private string _ReplacementItemName = "";
            private string _ReplacementItemOriginalText = "";
            private string _ReplacementItemNewText = "";
            

            public string ReplacementItemName
            {
                get { return _ReplacementItemName; }    set { _ReplacementItemName = value; }   
            }
            public string ReplacementItemOriginalText
            {
                get { return _ReplacementItemOriginalText; }
                set { _ReplacementItemOriginalText = value; }
            }
            public string ReplacementItemNewText
            {
                get { return _ReplacementItemNewText; } 
                set { _ReplacementItemNewText = value;}
            }
      
            

            public static AnonymizationItem buildNew(ReplacementItem repItem,string originalText) { 
                AnonymizationItem newItem = new AnonymizationItem();
                newItem.ReplacementItemName = repItem.Name;
                newItem.ReplacementItemOriginalText = originalText;
                if (repItem.UseRandom)
                {
                    newItem.ReplacementItemNewText = repItem.ReplacementText.Replace("##PLACEHOLDER##", AnonymizationItem.GetRandomString(repItem.RandomLength));
                }
                else {
                    newItem.ReplacementItemNewText = repItem.ReplacementText.Replace("##PLACEHOLDER##", repItem.ReplacementString);
                }
                return newItem;
            }
            public static string GetRandomString(int length)
            {
                Random random = new Random();   
                const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
                var chars = Enumerable.Range(0, length)
                    .Select(x => pool[random.Next(0, pool.Length)]);
                return new string(chars.ToArray()); 
            }
        }


    }



    internal class Configuration
    {
        private List<Core.ReplacementItem> _replacementsItems = new List<Core.ReplacementItem>();
        private bool _multiCore = false;
        private bool _anonymizeDirectoryStructure = false;
        private long _maxFileInMemoryMB = 20;

        public bool AnonymizeDirectoryStructure 
        {
            get { return _anonymizeDirectoryStructure; }
            set { _anonymizeDirectoryStructure = value; }
        }
        public bool MultiCore
        {
            get { return _multiCore; }
            set { _multiCore = value; }
        }

        public long MaxFileInMemoryMB
        {
            get { return _maxFileInMemoryMB; }
            set { _maxFileInMemoryMB = value; }
        }
        public List<Core.ReplacementItem> ReplacementItems
        {
            get { return _replacementsItems; }
            set { _replacementsItems = value; }
        }

        public static Configuration LoadConfiguration(string fileName) { 
            Configuration newConf = new Configuration(); ;
            string jsonString = "";
            try
            {
                StreamReader stream = new StreamReader(fileName);
                jsonString = stream.ReadToEnd();
            }
            catch (Exception ex) {
                Console.WriteLine("Error loading configuration file. Exception was: " + ex.Message);
                Environment.Exit(1);
            }

            try {
                newConf=JsonSerializer.Deserialize<Configuration>(jsonString);

            }catch (Exception ex)
            {
                Console.WriteLine("Error loading configuration file. Exception was: " + ex.Message);
                Environment.Exit(1);
            }
            if(newConf != null) {
                return newConf;
            }
            else
            {
                return new Configuration(); 
            }
            
        }
    }
}
