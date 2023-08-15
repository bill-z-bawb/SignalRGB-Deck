// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Win32;

var pluginName = string.Empty;
var binPath = string.Empty;
var distroToolPath = string.Empty;

RenderTitle();

try
{
    // Validate
    Validate(out pluginName, out binPath, out distroToolPath);
    
    // Kill StreamDeck
    BlockUntilStreamDeckProcessesKilled();

    // build the new sdplugin
    var pluginFile = BuildNewSdPlugin(
        pluginName, 
        new DirectoryInfo(binPath),
        new FileInfo(distroToolPath));

    // extract it and copy it into place
    InstallSdPlugin(pluginFile);

    //restart
    RestartStreamDeck(true);

    // end
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Done! Plugin \"{pluginName}\" installed successfully, the UI will show momentarily...");
}
catch (Exception e)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Failed! Plugin \"{pluginName}\" could not be deployed! {e.Message}");
    Console.ResetColor();

    if (e is ArgumentException)
    {
        DumpHelpToConsole();
    }
}

Console.ResetColor();
RenderHr();
Environment.Exit(0);

// Helpers
void Validate(out string name, out string bin, out string distroTool)
{
    if (args.Length < 3)
        throw new ArgumentException("StreamDeck Deployer requires 3 cli args!");

    name = args[0];
    bin = args[1];
    distroTool = args[2];

    if (!Directory.Exists(binPath))
        throw new ArgumentException($"Failed to find build artifacts folder at \"{binPath}\"");

    if (!File.Exists(distroTool))
        throw new ArgumentException($"Failed to find Elgato distro tool at \"{distroTool}\"! See usage below for download URL.");
}

const int ConsoleWidth = 120;
void RenderTitle()
{
    RenderHr();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("== S T R E A M D E C K   P L U G I N   D E P L O Y E R ".PadRight(ConsoleWidth, '='));
    Console.ResetColor();
    RenderHr();
}

void RenderHr()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("".PadRight(ConsoleWidth, '='));
    Console.ResetColor();
}

void DumpHelpToConsole()
{
    Console.WriteLine("\nUsage Details:\n");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("StreamDeckDeployer.exe ");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\"[plugin name]\" \"[bin artifacts path]\" \"[elgato distro tool path]\"\n");
    Console.ResetColor();
    Console.WriteLine("Arg details (listed in required order):\n");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(" * [plugin name]:             Reverse DNS syntax name of plugin\n" +
                      "                              e.g. \"com.myname.mypluginname\" (no sdtools extensions)\n\n" +
                      " * [bin artifacts path]:      The absolute path to the plugin's VS bin output folder\n" +
                      "                              e.g. \"C:\\Code\\MyPlugin\\bin\\Debug\"\n" +
                      "                              tip: for automation with build events use: \"$(TargetDir)\"\n\n" +
                      " * [elgato distro tool path]: The absolute path to the Elgato plugin packaging tool\n" +
                      "                              It can be downloaded here: https://docs.elgato.com/sdk/icon-packs/packaging\n" +
                      "                              e.g. \"C:\\Code\\StreamDeckTools\\DistributionToolWindows\\DistributionTool.exe\"" +
                      "");
    Console.ResetColor();
}
FileInfo BuildNewSdPlugin(string pluginName, DirectoryInfo buildArtifacts, FileInfo elgatoDistroTool)
{
    Console.WriteLine($"Building StreamDeck plugin \"{pluginName}\"...");
    var sdPluginFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{pluginName}.streamDeckPlugin"));
    if (sdPluginFile.Exists)
        sdPluginFile.Delete();

    ShellExecuteSync(elgatoDistroTool.FullName, $"-b -i {buildArtifacts.FullName} -o {Path.GetTempPath()}");
    return sdPluginFile;
}

void BlockUntilStreamDeckProcessesKilled()
{
    Console.WriteLine("Checking for running StreamDeck processes...");
    while (IsStreamDeckAppRunning(out var procs) > 0)
    {
        Console.WriteLine("Running StreamDeck processes found, killing...");
        foreach (var proc in procs)
        {
            try
            {
                Console.WriteLine($"Killing StreamDeck process \"{proc.ProcessName}\" (PID: {proc.Id})...");
                proc.Kill(true);
                Thread.Sleep(1000);
            }
            catch
            { }
        }
    }
    Console.WriteLine("StreamDeck processes are D-E-D...");
}

const string StreamDeckRegistryRoot = @"HKEY_CURRENT_USER\Software\Elgato Systems GmbH\StreamDeck\";
void RestartStreamDeck(bool ensureHtmlDebug = false)
{
    Console.WriteLine("Restarting StreamDeck process...");
    if (ensureHtmlDebug)
    {
        Console.WriteLine("Ensure StreamDeck starts with HTML debug server enabled...");
        Registry.SetValue(StreamDeckRegistryRoot, "html_remote_debugging_enabled", 1, RegistryValueKind.DWord);
    }

    var streamDeckAppPath = Registry.GetValue(StreamDeckRegistryRoot, "Folder", null) as string;
    if (streamDeckAppPath == null)
    {
        throw new Exception("Unable to find the StreamDeck application location in the registry!");
    }

    var streamDeckExePath = new FileInfo(Path.Combine(streamDeckAppPath, @"StreamDeck\StreamDeck.exe"));
    Console.WriteLine($"Start StreamDeck using \"{streamDeckExePath.FullName}\"...");
    ShellExecuteAsync(streamDeckExePath.FullName);

    Console.WriteLine("Started StreamDeck process, awaiting load...");
    while (IsStreamDeckAppRunning(out _) == 0)
    {
        Thread.Sleep(200);
    }
}

int IsStreamDeckAppRunning(out Process[] streamDeckProcs)
{
    var running = Process.GetProcesses();
    streamDeckProcs = running.Where(p => p.ProcessName.Equals("StreamDeck", StringComparison.InvariantCultureIgnoreCase)).ToArray();
    return streamDeckProcs.Length;
}

void InstallSdPlugin(FileInfo pluginArchive)
{
    Console.WriteLine($"Start install for plugin archive \"{pluginArchive.Name}\"...");

    var pluginName = Path.GetFileNameWithoutExtension(pluginArchive.Name);
    var pluginOutputPath = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"Elgato\StreamDeck\Plugins", $"{pluginName}.sdPlugin"));

    if (pluginOutputPath.Exists)
    {
        Console.WriteLine("Removing existing plugin...");
        pluginOutputPath.Delete(true);
    }
    
    // extract (install)
    Console.WriteLine($"Extracting plugin archive \"{pluginArchive.Name}\" to plugin install folder...");
    ZipFile.ExtractToDirectory(pluginArchive.FullName, pluginOutputPath.Parent.FullName);
    Console.WriteLine($"Installed {pluginOutputPath.GetFiles("*", SearchOption.AllDirectories).Length} files to folder \"{pluginOutputPath.Name}\"...");
}

Process ShellExecuteAsync(string executeThis, string? args = null)
{
    var sdProc = new Process();
    sdProc.EnableRaisingEvents = true;
    sdProc.StartInfo.CreateNoWindow = true;
    sdProc.StartInfo.FileName = executeThis;
    sdProc.StartInfo.Arguments = args;
    sdProc.StartInfo.UseShellExecute = true;
    sdProc.StartInfo.RedirectStandardOutput = false;
    sdProc.StartInfo.RedirectStandardError = false;
    sdProc.Start();
    return sdProc;
}

Process ShellExecuteSync(string executeThis, string? args = null)
{
    var sdProc = new Process();
    sdProc.EnableRaisingEvents = true;
    sdProc.StartInfo.CreateNoWindow = false;
    sdProc.StartInfo.FileName = executeThis;
    sdProc.StartInfo.Arguments = args;
    sdProc.StartInfo.UseShellExecute = false;
    sdProc.StartInfo.RedirectStandardOutput = false;
    sdProc.StartInfo.RedirectStandardError = false;
    sdProc.Start();
    sdProc.WaitForExit();
    return sdProc;
}
