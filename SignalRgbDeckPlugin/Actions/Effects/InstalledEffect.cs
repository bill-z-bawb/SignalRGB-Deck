using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal class InstalledEffect
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        public InstalledEffect(string effectId, string effectName)
        {
            Id = effectId;
            Name = effectName;
        }
    }
}