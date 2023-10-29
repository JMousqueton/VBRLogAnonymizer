using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using vbrLogAnon;

string _sourceDir = "";
string _destinationDir = "";
string _configFile = Environment.CurrentDirectory + @"\config.json";

Console.WriteLine("Veeam Backup logs Anonymizer C# edition v0.1");
Console.WriteLine();
if (CheckArgs())
{
    Configuration newConf = Configuration.LoadConfiguration(_configFile);
    Console.WriteLine("Loaded configuration from " + _configFile);
    Console.WriteLine("Source directory : " + _sourceDir);
    Console.WriteLine("Destination directory : " + _destinationDir);

    if (newConf.MultiCore)
    {
        Console.WriteLine("Multicore enabled. Possible high memory usage");
    }
    else
    {
        Console.WriteLine("Multicore disabled. Low memory usage");
    }
    Console.WriteLine("Maximum in-memory file size: " + newConf.MaxFileInMemoryMB + "MB");
    Core.DoWork(newConf.MultiCore, _sourceDir,_destinationDir, newConf);
}


bool CheckArgs()
{
    int goodScore = 2;
    int score = 0;

    if (Environment.GetCommandLineArgs().Length < 5)
    {
        Core.ShowUsage();
    }

    try
    {
        for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
        {
            if (Environment.GetCommandLineArgs()[i] == "-s")
            {
                if (Environment.GetCommandLineArgs()[i + 1].StartsWith("-"))
                {
                    Console.WriteLine("-s should be followed by source directory");
                    Core.ShowUsage();
                }
                else
                {
                    if (Directory.Exists(Environment.GetCommandLineArgs()[i + 1])){
                        _sourceDir = Environment.GetCommandLineArgs()[i + 1];
                        score++;
                    }
                    else
                    {
                        Console.WriteLine("Argument check: source directory does not exists");
                        Core.ShowUsage();
                    }
                }
            }
            if (Environment.GetCommandLineArgs()[i] == "-d")
            {
                if (Environment.GetCommandLineArgs()[i + 1].StartsWith("-"))
                {
                    Console.WriteLine("-d should be followed by destination directory");
                    Core.ShowUsage();
                }
                else
                {
                    if (Directory.Exists(Environment.GetCommandLineArgs()[i + 1])){
                        _destinationDir = Environment.GetCommandLineArgs()[i + 1];
                        score++;
                    }
                    else
                    {
                        Console.WriteLine("Argument check:  destination directory does not exists");
                        Core.ShowUsage();
                    }
                }
            }
            if (Environment.GetCommandLineArgs()[i] == "-c")
            {
                if (Environment.GetCommandLineArgs()[i + 1].StartsWith("-"))
                {
                    Console.WriteLine("-c should be followed by absolute path to Json configuration file");
                    Core.ShowUsage();
                }
                else
                {
                    if (File.Exists(Environment.GetCommandLineArgs()[i + 1]))
                    {
                        _configFile = Environment.GetCommandLineArgs()[i + 1];
                    }
                    else
                    {
                        Console.WriteLine("Argument check: configuration file does not exists");
                        Core.ShowUsage();
                    }
                }
            }
        }
    }
    catch {
        Console.WriteLine("Error parsing arguments, please review usage guidelines");
        Core.ShowUsage();
    }
    if (score == goodScore) {
        return true;
    }

    return false;
}