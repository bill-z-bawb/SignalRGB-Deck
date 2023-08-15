using System;
using System.Collections.Generic;
using System.Linq;
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

        public string PropsAsApplicationUrlArgString()
        {
            var args = new StringBuilder();
            var argPrefix = "?";
            foreach (var effectProperty in Properties)
            {
                var val = string.IsNullOrWhiteSpace(effectProperty.Value) ? effectProperty.Default : effectProperty.Value;
                args.Append($"{argPrefix}{Uri.EscapeDataString(effectProperty.PropertyName)}={Uri.EscapeDataString(val)}");
                argPrefix = "&";
            }
            return args.ToString();
        }
    }
}