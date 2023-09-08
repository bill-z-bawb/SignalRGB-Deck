using System.Collections.Generic;
using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    public class LayoutActionSettings
    {
        public static LayoutActionSettings CreateDefaultSettings()
        {
            var instance = new LayoutActionSettings
            {
                SelectedLayoutId = string.Empty,
                UserLayouts = new List<UserLayout>(),
            };
            return instance;
        }

        [JsonProperty(PropertyName = "selectedLayoutId")]
        public string SelectedLayoutId { get; set; }

        [JsonProperty(PropertyName = "selectedLayout")]
        public UserLayout SelectedLayout { get; set; }

        [JsonProperty(PropertyName = "userLayouts")]
        public List<UserLayout> UserLayouts { get; set; }
    }
}