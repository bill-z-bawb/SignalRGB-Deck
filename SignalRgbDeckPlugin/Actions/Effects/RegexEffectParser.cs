using System;
using System.Text.RegularExpressions;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal class RegexEffectParser : IEffectParser
    {
        private const string EffectTitlePattern = @"<title>(.+)</title>";
        private const string EffectPropertyPattern = @"<meta\s*(property=[\S\s]+?)\s*/?>";
        private const string EffectAttributePropertyPattern = @"{0}\s*=\s*""(.+?)""";

        public bool TryParse(string effectHtmlContent, string effectId, out InstalledEffectDetail effect)
        {
            effect = null;
            try
            {
                // start bit-bangin'
                var titleMatch = new Regex(EffectTitlePattern, RegexOptions.Multiline).Match(effectHtmlContent);
                var effectTitle = titleMatch.Success ? titleMatch.Groups[1].Value : effectId;

                effect = new InstalledEffectDetail(effectId, effectTitle);
                
                var effectProps = new Regex(EffectPropertyPattern, RegexOptions.Multiline).Matches(effectHtmlContent);
                if (effectProps.Count == 0)
                    return true;

                foreach (Match effectProp in effectProps)
                {
                    var foundPropElementText = effectProp.Groups[1].Value;
                    var foundProp = new EffectProperty();

                    foreach (var propName in EffectsHelper.KnownEffectAttributesList)
                    {
                        var foundAttribute = new Regex(string.Format(EffectAttributePropertyPattern, propName), RegexOptions.Multiline)
                            .Match(foundPropElementText);
                        if (!foundAttribute.Success)
                            continue;

                        foundProp.SetPropertyAttribute(propName, foundAttribute.Groups[1].Value);
                    }

                    effect.Properties.Add(foundProp);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"RegexPropParser failed! {ex.Message}");
            }

            return false;
        }
    }
}