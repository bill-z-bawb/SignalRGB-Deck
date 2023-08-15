using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const string EffectTitlePattern = @"<title>(.+)</title>";
        private const string EffectPropertyPattern = @"<meta\s*(property=[\S\s]+?)\s*/?>";
        private const string EffectAttributePropertyPattern = @"{0}\s*=\s*""(.+?)""";
        private static readonly string[] KnownEffectAttributesList = new[]
        {
            "property",
            "label",
            "type",
            "min",
            "max",
            "values",
            "default",
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

            var effectDetailsList = di
                .GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Where(EffectsDirectoryHasEffect)
                .Select(EffectFromEffectDirectory)
                .OrderBy(e => e.Name)
                .ToList();

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

            // start bit-bangin'
            var titleMatch = new Regex(EffectTitlePattern, RegexOptions.Multiline).Match(effectHtmlContent);
            var effectTitle = titleMatch.Success ? titleMatch.Groups[1].Value : effectFolder.Name;

            var installedEffect = new InstalledEffectDetail(effectFolder.Name, effectTitle);

            var effectProps = new Regex(EffectPropertyPattern, RegexOptions.Multiline).Matches(effectHtmlContent);
            if (effectProps.Count == 0)
                return installedEffect;

            foreach (Match effectProp in effectProps)
            {
                var foundPropElementText = effectProp.Groups[1].Value;
                var foundProp = new EffectProperty();

                foreach (var propName in KnownEffectAttributesList)
                {
                    var foundAttribute = new Regex(string.Format(EffectAttributePropertyPattern, propName), RegexOptions.Multiline)
                        .Match(foundPropElementText);
                    if (!foundAttribute.Success)
                        continue;

                    foundProp.SetPropertyAttribute(propName, foundAttribute.Groups[1].Value);
                }

                installedEffect.Properties.Add(foundProp);
            }

            return installedEffect;
        }
    }
}