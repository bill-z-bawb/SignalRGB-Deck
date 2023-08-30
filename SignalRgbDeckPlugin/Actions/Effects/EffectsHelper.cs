using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal static class EffectsHelper
    {
        internal static Dictionary<string, InstalledEffectDetail> EffectsDatabase = new Dictionary<string, InstalledEffectDetail>();
        internal static List<InstalledEffectDetail> Effects => EffectsDatabase.Select(kv => kv.Value).ToList();
        internal static List<InstalledEffect> EffectSummaries => Effects.Select(e => e.ToInstalledEffect()).ToList();
        internal static string EffectsPath = string.Empty;

        private const string EffectHtmlFileName = "effect.html";
        internal static readonly string[] KnownEffectAttributesList = new[]
        {
            "property",
            "label",
            "type",
            "min",
            "max",
            "values",
            "default",
        };

        private static readonly List<IEffectParser> EffectParsers = new List<IEffectParser>()
        {
            new HtmlEffectParser(),   // primary parser
            new RegexEffectParser(),  // fallback parser for super sloppy HTML
        };

        internal static InstalledEffectDetail EffectLookup(string forEffectId)
        {
            if (!EffectsDatabase?.Any() ?? true)
            {
                RefreshEffectsDatabase();
            }

            return EffectsDatabase[forEffectId];
        }

        internal static void RefreshEffectsDatabase()
        {
            EffectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"WhirlwindFX\SignalRgb\cache\effects");
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Made effects path => {EffectsPath}");
            var di = new DirectoryInfo(EffectsPath);

            if (!di.Exists)
                return;

            var effectDetailsList = new List<InstalledEffectDetail>();
            foreach (var effectFolder in di.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    if (!EffectsDirectoryHasEffect(effectFolder))
                        continue;

                    effectDetailsList.Add(EffectFromEffectDirectory(effectFolder));
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, $"Failed to parse effect \"{effectFolder.Name}\"! {ex.Message}");
                }
            }
            effectDetailsList = effectDetailsList.OrderBy(e => e.Name).ToList();

            // build global effect db
            EffectsDatabase = new Dictionary<string, InstalledEffectDetail>();
            effectDetailsList.ForEach(d => EffectsDatabase.Add(d.Id, d));
        }

        internal static bool EffectsDirectoryHasEffect(DirectoryInfo effectFolder)
        {
            return effectFolder.GetFiles("*", SearchOption.TopDirectoryOnly)
                .Any(f => f.Name.Equals(EffectHtmlFileName, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static InstalledEffectDetail EffectFromEffectDirectory(DirectoryInfo effectFolder)
        {
            var effectDoc = new FileInfo(Path.Combine(effectFolder.FullName, EffectHtmlFileName));
            if (!effectDoc.Exists)
                return null;

            var effectHtmlContent = File.ReadAllText(effectDoc.FullName);

            foreach (var propParser in EffectParsers)
            {
                if (!propParser.TryParse(effectHtmlContent, effectFolder.Name, out var parsedEffect))
                    continue;

                // success
                return parsedEffect;
            }

            throw new Exception($"Failed to parse effect in folder \"{effectFolder.FullName}\"!");
        }
    }
}

   