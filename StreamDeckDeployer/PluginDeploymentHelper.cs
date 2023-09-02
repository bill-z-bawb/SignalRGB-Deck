using System.Diagnostics;

namespace StreamDeckDeployer
{
    internal static class PluginDeploymentHelper
    {
        private const int ConsoleWidth = 120;
        private const int HelpVarNamePadding = 20;

        public static void RenderTitle()
        {
            RenderHr();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("== S T R E A M D E C K   P L U G I N   D E P L O Y E R ".PadRight(ConsoleWidth, '='));
            Console.ResetColor();
            RenderHr();
        }

        public static void RenderHr(char c = '=', ConsoleColor color = ConsoleColor.Yellow)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine("".PadRight(ConsoleWidth, c));
            Console.ForegroundColor = orig;
        }

        public static string? GetArgValue(string argKey, string[] args)
        {
            var argKv = args.FirstOrDefault(a => a.StartsWith($"/{argKey}"));
            if (string.IsNullOrWhiteSpace(argKv)) return null;

            var argParts = argKv.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (argParts.Length == 1)
                return string.Empty;

            var argValPats = argParts.Skip(1);
            return string.Join(string.Empty, argValPats);
        }

        public static bool HasArgKey(string argKey, string[] args)
        {
            // the empty string is valid
            return GetArgValue(argKey, args) != null;
        }

        public static string GetHelpVarName(string name)
        {
            return $" * {name}:".PadRight(HelpVarNamePadding, ' ');
        }

        public static string GetHelpVarAlignSpacer()
        {
            return "".PadRight(HelpVarNamePadding, ' ');
        }

        public static Process ShellExecuteAsync(string executeThis, string? args = null)
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

        public static Process ShellExecuteSync(string executeThis, string? args = null)
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

        public static void OpenWebsite(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
