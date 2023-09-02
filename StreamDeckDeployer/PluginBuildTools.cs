using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;
using Helper = StreamDeckDeployer.PluginDeploymentHelper;

namespace StreamDeckDeployer
{
    internal static class PluginBuildTools
    {
        private const string PluginNameKey = "name";
        private const string PluginBinarySourceKey = "bin";
        private const string PluginDistroToolKey = "distroTool";
        private const string InstallPluginKey = "install";
        private const string InstallOutputFolderKey = "output";
        private const string HelpKey = "help";

        public static void Start(string[] args)
        {
            var pluginName = string.Empty;
            try
            {
                if (Helper.HasArgKey(HelpKey, args))
                {
                    DumpHelpToConsole();
                    return;
                }

                // Validate
                Validate(args, out pluginName, out var binPath, out var distroToolPath, out var installPlugin, out var builtPluginPath);

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
        }

        private static void Validate(string[] args, out string name, out string bin, out string distroTool, out bool shouldInstallPlugin, out string pluginOutPath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            try
            {
                name = Helper.GetArgValue(PluginNameKey, args);
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException($"The \"/{PluginNameKey}\" is missing! This arg specifies the reverse DNS name of the plugin.");
                Console.WriteLine($"{Helper.GetHelpVarName(PluginNameKey)}{name}");

                bin = Helper.GetArgValue(PluginBinarySourceKey, args);
                if (string.IsNullOrWhiteSpace(bin))
                    throw new ArgumentException($"The \"/{PluginBinarySourceKey}\" is missing! This arg specifies the location path of plugin's binary components.");
                Console.WriteLine($"{Helper.GetHelpVarName(PluginBinarySourceKey)}{bin}");

                distroTool = Helper.GetArgValue(PluginDistroToolKey, args);
                distroTool = string.IsNullOrWhiteSpace(distroTool)
                    ? GetDefaultDistroToolPath().FullName
                    : distroTool;
                Console.WriteLine($"{Helper.GetHelpVarName(PluginDistroToolKey)}{distroTool}");

                shouldInstallPlugin = Helper.HasArgKey(InstallPluginKey, args);
                Console.WriteLine($"{Helper.GetHelpVarName(InstallPluginKey)}{shouldInstallPlugin}");

                pluginOutPath = Helper.GetArgValue(InstallOutputFolderKey, args);
                pluginOutPath = !string.IsNullOrWhiteSpace(pluginOutPath)
                    ? pluginOutPath
                    : Path.GetTempPath();
                pluginOutPath = Path.Combine(pluginOutPath, $"{name}.streamDeckPlugin");
                Console.WriteLine($"{Helper.GetHelpVarName(InstallOutputFolderKey)}{pluginOutPath}");

                if (!Directory.Exists(bin))
                    throw new ArgumentException($"Failed to find build artifacts folder at \"{bin}\"");

                if (!File.Exists(distroTool))
                    throw new ArgumentException($"Failed to find Elgato distro tool at \"{distroTool}\"! See usage below for download URL.");
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static FileInfo GetDefaultDistroToolPath()
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

        private static void BuildNewSdPlugin(string pluginName, DirectoryInfo buildArtifacts, FileInfo elgatoDistroTool, FileInfo pluginOutputFile)
        {
            Console.WriteLine($"\nBuilding StreamDeck plugin \"{pluginName}\" to \"{pluginOutputFile.FullName}\"...\n");
            if (pluginOutputFile.Exists)
                pluginOutputFile.Delete();

            if (!pluginOutputFile.Directory.Exists)
                pluginOutputFile.Directory.Create();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Helper.RenderHr('-', ConsoleColor.DarkGray);
            Console.WriteLine("Elgato Distribution Tool Output:");
            Helper.ShellExecuteSync(elgatoDistroTool.FullName, $"-b -i {buildArtifacts.FullName} -o {pluginOutputFile.DirectoryName}");
            Helper.RenderHr('-', ConsoleColor.DarkGray);
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void BlockUntilStreamDeckProcessesKilled()
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
        private static void RestartStreamDeck(bool ensureHtmlDebug = false)
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
            Helper.ShellExecuteAsync(streamDeckExePath.FullName);

            Console.WriteLine("Started StreamDeck process, awaiting load...");
            while (IsStreamDeckAppRunning(out _) == 0)
            {
                Thread.Sleep(200);
            }
        }

        private static int IsStreamDeckAppRunning(out Process[] streamDeckProcs)
        {
            var running = Process.GetProcesses();
            streamDeckProcs = running.Where(p => p.ProcessName.Equals("StreamDeck", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return streamDeckProcs.Length;
        }

        private static void InstallSdPlugin(FileInfo pluginArchive)
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

        private static void DumpHelpToConsole()
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
            Console.WriteLine($"{Helper.GetHelpVarName(PluginNameKey)}[REQUIRED] - Reverse DNS syntax name of plugin\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}e.g. /{PluginNameKey}=\"com.myname.mypluginname\" (no sdtools extensions)\n\n" +
                              $"{Helper.GetHelpVarName(PluginBinarySourceKey)}[REQUIRED] - The absolute path to the plugin's VS bin output folder\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}e.g. /{PluginBinarySourceKey}=\"C:\\Code\\MyPlugin\\bin\\Debug\"\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}tip: for automation with build events use: \"$(TargetDir)\"\n\n" +
                              $"{Helper.GetHelpVarName(PluginDistroToolKey)}[OPTIONAL] - The absolute path to the Elgato plugin packaging tool\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}It can be downloaded here: https://docs.elgato.com/sdk/icon-packs/packaging\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}e.g. /{PluginDistroToolKey}=\"C:\\Code\\StreamDeckTools\\DistributionToolWindows\\DistributionTool.exe\"\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}Omitting this arg will use the default Elgato Distribution tool\n\n" +
                              $"{Helper.GetHelpVarName(InstallPluginKey)}[OPTIONAL] - Whether or not the Deployer Tool will also install after building\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}e.g. /{InstallPluginKey}\n\n" +
                              $"{Helper.GetHelpVarName(InstallOutputFolderKey)}[OPTIONAL] - The output folder of the built Elgato \".streamDeckPlugin\" file\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}e.g. /{InstallOutputFolderKey}=\"C:\\MyPlugin\\OutputFolder\"\n" +
                              $"{Helper.GetHelpVarAlignSpacer()}Omitting this arg will emit the plugin file to a temporary directory\n" +
                              "");
            Console.ResetColor();
        }
    }
}
