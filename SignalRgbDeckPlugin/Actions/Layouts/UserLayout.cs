using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    internal class UserLayout
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        public UserLayout(string layoutId, string layoutName)
        {
            Id = layoutId;
            Name = layoutName;
        }
    }
}