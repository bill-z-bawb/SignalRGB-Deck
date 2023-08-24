// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Win32;

const int consoleWidth = 120;
const int helpVarNamePadding = 20;

const string PluginNameKey = "name";
const string PluginBinarySourceKey = "bin";
const string PluginDistroToolKey = "distroTool";
const string InstallPluginKey = "install";
const string InstallOutputFolderKey = "output";
const string HelpKey = "help";

var pluginName = string.Empty;
var binPath = string.Empty;
var distroToolPath = string.Empty;
var installPlugin = false;
var builtPluginPath = string.Empty;

RenderTitle();

if (HasArgKey(HelpKey))
{
    DumpHelpToConsole();
    return;
}

try
{
    // Validate
    Validate(out pluginName, out binPath, out distroToolPath, out installPlugin, out builtPluginPath);

    if (installPlugin)
    {
        // Kill StreamDeck
        BlockUntilStreamDeckProcessesKilled();
    }
    
    // build the new sdplugin
    var pluginFile = new FileInfo(builtPluginPath);
    BuildNewSdPlugin(
        pluginName, 
        new DirectoryInfo(binPath),
        new FileInfo(distroToolPath),
        pluginFile);

    if (installPlugin)
    {
        // extract it and copy it into place
        InstallSdPlugin(pluginFile);

        //restart
        RestartStreamDeck(true);

        // end
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Done! Plugin \"{pluginName}\" built and installed successfully, the UI will show momentarily...");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Done! Plugin \"{pluginName}\" built successfully at\n  => {builtPluginPath}");
    }
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
void Validate(out string name, out string bin, out string distroTool, out bool shouldInstallPlugin, out string pluginOutPath)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    try
    {
        name = GetArgValue(PluginNameKey);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"The \"/{PluginNameKey}\" is missing! This arg specifies the reverse DNS name of the plugin.");
        Console.WriteLine($"{GetHelpVarName(PluginNameKey)}{name}");
        
        bin = GetArgValue(PluginBinarySourceKey);
        if (string.IsNullOrWhiteSpace(bin))
            throw new ArgumentException($"The \"/{PluginBinarySourceKey}\" is missing! This arg specifies the location path of plugin's binary components.");
        Console.WriteLine($"{GetHelpVarName(PluginBinarySourceKey)}{bin}");

        distroTool = GetArgValue(PluginDistroToolKey);
        distroTool = string.IsNullOrWhiteSpace(distroTool)
            ? GetDefaultDistroToolPath().FullName
            : distroTool;
        Console.WriteLine($"{GetHelpVarName(PluginDistroToolKey)}{distroTool}");

        shouldInstallPlugin = HasArgKey(InstallPluginKey);
        Console.WriteLine($"{GetHelpVarName(InstallPluginKey)}{shouldInstallPlugin}");

        pluginOutPath = GetArgValue(InstallOutputFolderKey);
        pluginOutPath = !string.IsNullOrWhiteSpace(pluginOutPath)
            ? pluginOutPath
            : Path.GetTempPath();
        pluginOutPath = Path.Combine(pluginOutPath, $"{pluginName}.streamDeckPlugin");
        Console.WriteLine($"{GetHelpVarName(InstallOutputFolderKey)}{pluginOutPath}");

        if (!Directory.Exists(binPath))
            throw new ArgumentException($"Failed to find build artifacts folder at \"{binPath}\"");

        if (!File.Exists(distroTool))
            throw new ArgumentException($"Failed to find Elgato distro tool at \"{distroTool}\"! See usage below for download URL.");
    }
    finally
    {
        Console.ResetColor();
    }
}

string? GetArgValue(string argKey)
{
    var argKv = args.FirstOrDefault(a => a.StartsWith($"/{argKey}"));
    if (string.IsNullOrWhiteSpace(argKv)) return null;

    var argParts = argKv.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (argParts.Length == 1)
        return string.Empty;

    var argValPats = argParts.Skip(1);
    return string.Join(string.Empty, argValPats);
}

bool HasArgKey(string argKey)
{
    // the empty string is valid
    return GetArgValue(argKey) != null;
}

FileInfo GetDefaultDistroToolPath()
{
    var currentPath = Environment.ProcessPath;
    var current = new DirectoryInfo(Path.GetDirectoryName(currentPath));
    while (true)
    {
        var distroTool = current
            .GetFiles("*.exe", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f => f.Name.Equals("DistributionTool.exe"));
        if (distroTool != null)
        {
            return distroTool;
        }

        current = new DirectoryInfo(Path.GetFullPath(Path.Combine(current.FullName, @"..\")));
    }
}

void RenderTitle()
{
    RenderHr();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("== S T R E A M D E C K   P L U G I N   D E P L O Y E R ".PadRight(consoleWidth, '='));
    Console.ResetColor();
    RenderHr();
}

void RenderHr(char c =  '=', ConsoleColor color = ConsoleColor.Yellow)
{
    var orig = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine("".PadRight(consoleWidth, c));
    Console.ForegroundColor = orig;
}

void DumpHelpToConsole()
{
    Console.WriteLine("\nUsage Details:\n");
    Console.WriteLine("To build and then deploy:");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("StreamDeckDeployer.exe ");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"/{PluginNameKey}=\"[name]\" /{PluginBinarySourceKey}=\"[path]\" /{PluginDistroToolKey}=\"[path]\" /{InstallPluginKey}\n");
    Console.ResetColor();
    Console.WriteLine("To build only:");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("StreamDeckDeployer.exe ");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"/{PluginNameKey}=\"[name]\" /{PluginBinarySourceKey}=\"[path]\" /{PluginDistroToolKey}=\"[path]\" /{InstallOutputFolderKey}=[path]\n");
    Console.ResetColor();
    Console.WriteLine("Arg details:\n");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"{GetHelpVarName(PluginNameKey)}[REQUIRED] - Reverse DNS syntax name of plugin\n" +
                      $"{GetHelpVarAlignSpacer()}e.g. /{PluginNameKey}=\"com.myname.mypluginname\" (no sdtools extensions)\n\n" +
                      $"{GetHelpVarName(PluginBinarySourceKey)}[REQUIRED] - The absolute path to the plugin's VS bin output folder\n" +
                      $"{GetHelpVarAlignSpacer()}e.g. /{PluginBinarySourceKey}=\"C:\\Code\\MyPlugin\\bin\\Debug\"\n" +
                      $"{GetHelpVarAlignSpacer()}tip: for automation with build events use: \"$(TargetDir)\"\n\n" +
                      $"{GetHelpVarName(PluginDistroToolKey)}[OPTIONAL] - The absolute path to the Elgato plugin packaging tool\n" +
                      $"{GetHelpVarAlignSpacer()}It can be downloaded here: https://docs.elgato.com/sdk/icon-packs/packaging\n" +
                      $"{GetHelpVarAlignSpacer()}e.g. /{PluginDistroToolKey}=\"C:\\Code\\StreamDeckTools\\DistributionToolWindows\\DistributionTool.exe\"\n" +
                      $"{GetHelpVarAlignSpacer()}Omitting this arg will use the default Elgato Distribution tool\n\n" +
                      $"{GetHelpVarName(InstallPluginKey)}[OPTIONAL] - Whether or not the Deployer Tool will also install after building\n" +
                      $"{GetHelpVarAlignSpacer()}e.g. /{InstallPluginKey}\n\n" +
                      $"{GetHelpVarName(InstallOutputFolderKey)}[OPTIONAL] - The output folder of the built Elgato \".streamDeckPlugin\" file\n" +
                      $"{GetHelpVarAlignSpacer()}e.g. /{InstallOutputFolderKey}=\"C:\\MyPlugin\\OutputFolder\"\n" +
                      $"{GetHelpVarAlignSpacer()}Omitting this arg will emit the plugin file to a temporary directory\n" +
                      "");
    Console.ResetColor();
}

string GetHelpVarName(string name)
{
    return $" * {name}:".PadRight(helpVarNamePadding, ' ');
}

string GetHelpVarAlignSpacer()
{
    return "".PadRight(helpVarNamePadding, ' ');
}

void BuildNewSdPlugin(string pluginName, DirectoryInfo buildArtifacts, FileInfo elgatoDistroTool, FileInfo pluginOutputFile)
{
    Console.WriteLine($"\nBuilding StreamDeck plugin \"{pluginName}\" to \"{pluginOutputFile.FullName}\"...\n");
    if (pluginOutputFile.Exists)
        pluginOutputFile.Delete();

    if (!pluginOutputFile.Directory.Exists)
        pluginOutputFile.Directory.Create();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    RenderHr('-', ConsoleColor.DarkGray);
    Console.WriteLine("Elgato Distribution Tool Output:");
    ShellExecuteSync(elgatoDistroTool.FullName, $"-b -i {buildArtifacts.FullName} -o {pluginOutputFile.DirectoryName}");
    RenderHr('-', ConsoleColor.DarkGray);
    Console.WriteLine();
    Console.ResetColor();
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
