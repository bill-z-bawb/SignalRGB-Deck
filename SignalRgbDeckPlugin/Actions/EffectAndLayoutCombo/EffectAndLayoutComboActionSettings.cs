using Newtonsoft.Json;
using SignalRgbDeckPlugin.Actions.Effects;
using SignalRgbDeckPlugin.Actions.Layouts;
using System.Collections.Generic;

namespace SignalRgbDeckPlugin.Actions.EffectAndLayoutCombo
{
    public class EffectAndLayoutComboActionSettings
    {
        public static EffectAndLayoutComboActionSettings CreateDefaultSettings()
        {
            var instance = new EffectAndLayoutComboActionSettings
            {
                // Effects
                SelectedEffectId = string.Empty,
                SelectedEffect = null,
                InstalledEffects = new List<InstalledEffect>(),
                // Layouts
                SelectedLayoutId = string.Empty,
                UserLayouts = new List<UserLayout>(),
            };
            return instance;
        }

        // Effect props
        [JsonProperty(PropertyName = "selectedEffectId")]
        public string SelectedEffectId { get; set; }

        [JsonProperty(PropertyName = "selectedEffect")]
        public InstalledEffectDetail SelectedEffect { get; set; }

        [JsonProperty(PropertyName = "installedEffects")]
        public List<InstalledEffect> InstalledEffects { get; set; }

        // Layout props
        [JsonProperty(PropertyName = "selectedLayoutId")]
        public string SelectedLayoutId { get; set; }

        [JsonProperty(PropertyName = "selectedLayout")]
        public UserLayout SelectedLayout { get; set; }

        [JsonProperty(PropertyName = "userLayouts")]
        public List<UserLayout> UserLayouts { get; set; }





















    }
}