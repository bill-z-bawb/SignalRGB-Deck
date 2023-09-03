using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal class InstalledEffectDetail : InstalledEffect
    {
        [JsonProperty(PropertyName = "properties")]
        public List<EffectProperty> Properties { get; private set; } = new List<EffectProperty>();

        public InstalledEffectDetail(string effectId, string effectName) : base(effectId, effectName)
        {

        }

        public void SetPropertyValue(string key, string val)
        {
            var matchingEffectProp = Properties.FirstOrDefault(p => p.PropertyName.Equals(key));
            if (matchingEffectProp == null)
                return;

            matchingEffectProp.Value = val;
        }

        public InstalledEffect ToInstalledEffect()
        {
            return this;
        }

        public string PropsAsApplicationUrlArgString(bool requestSilentLaunch)
        {
            var args = new StringBuilder();
            var argPrefix = "?";
            foreach (var effectProperty in Properties)
            {
                var val = string.IsNullOrWhiteSpace(effectProperty.Value) ? effectProperty.Default : effectProperty.Value;
                args.Append($"{argPrefix}{Uri.EscapeDataString(effectProperty.PropertyName)}={Uri.EscapeDataString(val)}");
                argPrefix = "&";
            }

            if (requestSilentLaunch)
            {
                args.Append($"{argPrefix}{SignalRgbKeypadBase.SilentLaunchRequest}");
            }

            return args.ToString();
        }

        private const string CachedEffectHtmlFileName = "effect.html";
        private static readonly List<IEffectParser> EffectParsers = new List<IEffectParser>()
        {
            new HtmlEffectParser(),   // primary parser
            new RegexEffectParser(),  // fallback parser for super sloppy HTML
        };

        public static InstalledEffectDetail EffectFromCacheDirectory(DirectoryInfo effectFolder)
        {
            var effectDoc = new FileInfo(Path.Combine(effectFolder.FullName, CachedEffectHtmlFileName));
            return EffectFromHtml(effectDoc, effectFolder.Name);
        }

        public static bool EffectsCacheDirectoryHasEffect(DirectoryInfo effectFolder)
        {
            return effectFolder.GetFiles("*", SearchOption.TopDirectoryOnly)
                .Any(f => f.Name.Equals(CachedEffectHtmlFileName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static InstalledEffectDetail EffectFromHtml(FileInfo effect, string effectId = null)
        {
            if (!effect.Exists)
                throw new FileNotFoundException($"Effect \"{effect.FullName}\" does not exist!", effect.FullName);

            effectId = effectId ?? effect.HashCode(MD5.Create());
            var effectHtmlContent = File.ReadAllText(effect.FullName);

            foreach (var propParser in EffectParsers)
            {
                if (!propParser.TryParse(effectHtmlContent, effectId, out var parsedEffect))
                    continue;

                // success
                return parsedEffect;
            }

            throw new Exception($"Failed to parse effect \"{effect.FullName}\"!");
        }
    }
}