using Newtonsoft.Json;
using SignalRgbDeckPlugin.Actions.Effects;
using SignalRgbDeckPlugin.Actions.Layouts;

namespace SignalRgbDeckPlugin.Actions.EffectAndLayoutCombo
{
    public class EffectAndLayoutComboActionSettings
    {
        public static EffectAndLayoutComboActionSettings CreateDefaultSettings()
        {
            var instance = new EffectAndLayoutComboActionSettings
            {
                EffectSettings = EffectActionSettings.CreateDefaultSettings(),
                LayoutSettings = LayoutActionSettings.CreateDefaultSettings(),
            };
            return instance;
        }

        [JsonProperty(PropertyName = "effectSettings")]
        public EffectActionSettings EffectSettings { get; set; }

        [JsonProperty(PropertyName = "layoutSettings")]
        public LayoutActionSettings LayoutSettings { get; set; }
    }
}