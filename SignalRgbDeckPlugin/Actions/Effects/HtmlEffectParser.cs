using System;
using System.Linq;
using BarRaider.SdTools;
using HtmlAgilityPack;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal class HtmlEffectParser : IEffectParser
    {
        public bool TryParse(string effectHtmlContent, string effectId, out InstalledEffectDetail effect)
        {
            effect = null;
            try
            {
                var h = new HtmlDocument();
                effectHtmlContent = effectHtmlContent.TrimStart().StartsWith("<html>")
                    ? effectHtmlContent
                    : $"<html>{effectHtmlContent}</html>";
                h.LoadHtml(effectHtmlContent);

                var title = h.DocumentNode.SelectNodes("/html/head/title/text()").FirstOrDefault()?.InnerText;
                var effectTitle = !string.IsNullOrWhiteSpace(title) ? title : effectId;

                effect = new InstalledEffectDetail(effectId, effectTitle);
                foreach (var node in h.DocumentNode.SelectNodes("//meta[@property]"))
                {
                    var foundProp = new EffectProperty();
                    foreach (var attribute in node.Attributes)
                    {
                        if (!EffectsHelper.KnownEffectAttributesList.Contains(attribute.Name))
                            continue;

                        foundProp.SetPropertyAttribute(attribute.Name, attribute.Value);
                    }
                    effect.Properties.Add(foundProp);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HtmlPropParser failed! {ex.Message}");
            }

            return false;
        }
    }
}