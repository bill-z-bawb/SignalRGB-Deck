using BarRaider.SdTools;

namespace SamplePlugin
{
    class Program
    {
        static void Main(string[] args)
        {
            //var rootDi = new DirectoryInfo(@"C:\Users\kenpe\AppData\Local\WhirlwindFX\SignalRgb\cache\effects");
            //var singleEffectTestLoad =
            //    PluginAction.EffectFromEffectDirectory(
            //        new DirectoryInfo(Path.Combine(rootDi.FullName, @"-M6kJJl-tlZKQLjDg2ng")));

            //var allEffectsTest = new List<InstalledEffectDetail>();
            
            //allEffectsTest.AddRange(rootDi.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(PluginAction.EffectsDirectoryHasEffect).Select(PluginAction.EffectFromEffectDirectory));

            //var allTypes = allEffectsTest.SelectMany(e => e.Properties).Select(p => p.Type).Distinct().OrderBy(t => t).ToList();
            //var json = allEffectsTest.First().ListValue;
            //var effectStuff = PluginAction.EffectFromEffectDirectory(new DirectoryInfo(@"C:\Users\kenpe\AppData\Local\WhirlwindFX\SignalRgb\cache\effects\-MCD91PcpkAuQtlFQw32"));

            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }
    }
}
