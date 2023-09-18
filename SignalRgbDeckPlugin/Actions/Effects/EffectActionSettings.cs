using System.Collections.Generic;
using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    public class EffectActionSettings
    {
        public static EffectActionSettings CreateDefaultSettings()
        {
            var instance = new EffectActionSettings
            {
                SelectedEffectId = string.Empty,
                SelectedEffect = null,
                InstalledEffects = new List<InstalledEffect>(),
            };
            return instance;
        }

        [JsonProperty(PropertyName = "selectedEffectId")]
        public string SelectedEffectId { get; set; }

        [JsonProperty(PropertyName = "selectedEffect")]
        public InstalledEffectDetail SelectedEffect { get; set; }

        [JsonProperty(PropertyName = "installedEffects")]
        public List<InstalledEffect> InstalledEffects { get; set; }
    }
}