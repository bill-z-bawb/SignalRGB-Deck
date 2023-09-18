using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal static class EffectsHelper
    {
        internal static Dictionary<string, InstalledEffectDetail> EffectsDatabase = new Dictionary<string, InstalledEffectDetail>();
        internal static List<InstalledEffectDetail> Effects => EffectsDatabase.Select(kv => kv.Value).ToList();
        internal static List<InstalledEffect> EffectSummaries => Effects.Select(e => e.ToInstalledEffect()).ToList();
        internal static string CachedEffectsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"WhirlwindFX\SignalRgb\cache\effects");
        internal static string InstalledEffectsPathRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"VortxEngine");
        internal static string InstalledEffectsPathTemplate => Path.Combine(InstalledEffectsPathRoot, 
            @"{0}\Signal-x64\Effects");

        internal static List<string> InstalledAppVersions
        {
            get
            {
                var di = new DirectoryInfo(InstalledEffectsPathRoot);
                return !di.Exists
                    ? new List<string>()
                    : di.GetDirectories("app-*", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(f => f.CreationTime)
                        .Select(d =>d.Name)
                        .ToList();
            }
        }
        
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

        internal static readonly string[] ValidEffectPresets = new[] { "a", "b", "c" };

        internal static bool IsValidPreset(string preset)
        {
            return !string.IsNullOrWhiteSpace(preset) &&
                   ValidEffectPresets.Any(p => p.Equals(preset, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static string BuildEffectUrlFromSettings(IEffectActionSettings settings)
        {
            var url = new StringBuilder();

            if (IsValidPreset(settings.SelectedEffectPreset))
            {
                url.Append("signalrgb://effect/applypreset/");
                url.Append(Uri.EscapeDataString(settings.SelectedEffect.Name));
                url.Append($"/{settings.SelectedEffectPreset}");
                url.Append($"?{SignalRgbKeypadBase.SilentLaunchRequest}");
            }
            else // use settings
            {
                url.Append("signalrgb://effect/apply/");
                url.Append(Uri.EscapeDataString(settings.SelectedEffect.Name));
                // add the effect's settings
                url.Append(settings.SelectedEffect.PropsAsApplicationUrlArgString(true));
            }

            return url.ToString();
        }

        internal static DirectoryInfo GetInstalledEffectsPathForAppVersion(DirectoryInfo appVer) =>
            GetInstalledEffectsPathForAppVersion(appVer.FullName);
        internal static DirectoryInfo GetInstalledEffectsPathForAppVersion(string appVer) =>
            new DirectoryInfo(string.Format(InstalledEffectsPathTemplate, appVer));

        internal static InstalledEffectDetail EffectLookup(string forEffectId)
        {
            if (string.IsNullOrWhiteSpace(forEffectId))
                return null;

            if (!EffectsDatabase?.Any() ?? true)
            {
                RefreshEffectsDatabase();
            }

            return EffectsDatabase[forEffectId];
        }

        internal static void RefreshEffectsDatabase()
        {
            // build global effect db
            EffectsDatabase = new Dictionary<string, InstalledEffectDetail>();

            // Since the same effect can appear as both cached and installed I guess prioritize cached...
            var allEffects = GetCachedEffects();

            var installedEffects = GetInstalledEffects();
            var newInstalledEffects = installedEffects.Where(e => allEffects.All(a => !a.Name.Equals(e.Name))).ToList();
            allEffects.AddRange(newInstalledEffects);

            allEffects = allEffects.OrderBy(e => e.Name).ToList();

            allEffects.ForEach(d => EffectsDatabase.Add(d.Id, d));
       }

        internal static List<InstalledEffectDetail> GetCachedEffects()
        {
            try
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Made effects path => {CachedEffectsPath}");
                var di = new DirectoryInfo(CachedEffectsPath);

                if (!di.Exists)
                    return new List<InstalledEffectDetail>();

                var effectDetailsList = new List<InstalledEffectDetail>();
                foreach (var effectFolder in di.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        if (!InstalledEffectDetail.EffectsCacheDirectoryHasEffect(effectFolder))
                            continue;

                        effectDetailsList.Add(InstalledEffectDetail.EffectFromCacheDirectory(effectFolder));
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN, $"Failed to parse effect \"{effectFolder.Name}\"! {ex.Message}");
                    }
                }

                return effectDetailsList;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to fetch cached effects! {ex.Message}");
            }

            return new List<InstalledEffectDetail>();
        }

        internal static List<InstalledEffectDetail> GetInstalledEffects()
        {
            try
            {
                var installedEffects = new List<InstalledEffectDetail>();
                // App folders in desc order (time created). For duplicate effects, fcfs
                foreach (var appVersion in InstalledAppVersions)
                {
                    var appEffectsFiles = GetInstalledEffectsPathForAppVersion(appVersion).GetFiles("*.html", SearchOption.AllDirectories);
                    foreach (var effectFile in appEffectsFiles)
                    {
                        var effect = InstalledEffectDetail.EffectFromHtml(effectFile);
                        if (installedEffects.Any(i => i.Name.Equals(effect.Name)))
                            continue;
                            
                        installedEffects.Add(effect);
                    }
                }
                
                return installedEffects;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to fetch installed effects! {ex.Message}");
            }

            return new List<InstalledEffectDetail>();
        }

        public static string HashCode(this FileInfo fi, HashAlgorithm cryptoService)
        {
            return HashCode(fi.FullName, cryptoService);
        }

        internal static string HashCode(string filePath, HashAlgorithm cryptoService)
        {
            // can be either MD5, SHA1, SHA256, SHA384 or SHA512
            using (cryptoService)
            {
                using (var fileStream = new FileStream(filePath,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.ReadWrite))
                {
                    var hash = cryptoService.ComputeHash(fileStream);
                    var hashString = Convert.ToBase64String(hash);
                    return hashString.TrimEnd('=');
                }
            }
        }
    }
}

   