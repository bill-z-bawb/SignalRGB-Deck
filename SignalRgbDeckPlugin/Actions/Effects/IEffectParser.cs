using System.Collections.Generic;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal interface IEffectParser
    {
        bool TryParse(string effectHtmlContent, string effectId, out InstalledEffectDetail effect);
    }
}