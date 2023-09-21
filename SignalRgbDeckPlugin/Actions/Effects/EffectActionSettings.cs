using System.Collections.Generic;
using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    public class EffectActionSettings : IEffectActionSettings
    {
        public static EffectActionSettings CreateDefaultSettings()
        {
            var instance = new EffectActionSettings
            {
                SelectedEffectId = string.Empty,
                SelectedEffectPreset = "none",
                SelectedEffect = null,
                InstalledEffects = new List<InstalledEffect>(),
            };
            return instance;
        }

        [JsonProperty(PropertyName = "selectedEffectPreset")]
        public string SelectedEffectPreset { get; set; } = "none";

        [JsonProperty(PropertyName = "selectedEffectId")]
        public string SelectedEffectId { get; set; }

        [JsonProperty(PropertyName = "selectedEffect")]
        public InstalledEffectDetail SelectedEffect { get; set; }

        [JsonProperty(PropertyName = "installedEffects")]
        public List<InstalledEffect> InstalledEffects { get; set; }
    }
}