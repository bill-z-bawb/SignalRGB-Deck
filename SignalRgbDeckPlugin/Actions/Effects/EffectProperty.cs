using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    internal class EffectProperty
    {
        [JsonProperty(PropertyName = "property")]
        public string PropertyName { get; set; }
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "values")]
        public List<string> Values { get; set; }
        [JsonProperty(PropertyName = "min")]
        public int Min { get; set; }
        [JsonProperty(PropertyName = "max")]
        public int Max { get; set; }
        [JsonProperty(PropertyName = "default")]
        public string Default { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        public void SetPropertyAttribute(string jsonKey, string value)
        {
            var pocoProp = GetMatchingJsonProperty(jsonKey);
            if (pocoProp == null)
                throw new ArgumentException($"JSON key \"{jsonKey}\" is not a know property attribute within {nameof(EffectProperty)}!");

            if (pocoProp.PropertyType == typeof(string))
            {
                pocoProp.SetValue(this, value);
            }
            else if (pocoProp.PropertyType == typeof(int))
            {
                pocoProp.SetValue(this, Convert.ToInt32(value));
            }
            else if (pocoProp.PropertyType == typeof(bool))
            {
                pocoProp.SetValue(this, Convert.ToBoolean(value));
            }
            else if (pocoProp.PropertyType == typeof(List<string>))
            {
                var valTokens = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
                var valList = new List<string>();
                valList.AddRange(valTokens);
                pocoProp.SetValue(this, valList);
            }
            else if (pocoProp.PropertyType == typeof(double))
            {
                pocoProp.SetValue(this, Convert.ToDouble(value));
            }
            else if (pocoProp.PropertyType == typeof(float))
            {
                pocoProp.SetValue(this, (float)Convert.ToDouble(value));
            }
            else if (pocoProp.PropertyType == typeof(short))
            {
                pocoProp.SetValue(this, Convert.ToInt16(value));
            }
            else if (pocoProp.PropertyType == typeof(long))
            {
                pocoProp.SetValue(this, Convert.ToInt64(value));
            }
        }

        private PropertyInfo GetMatchingJsonProperty(string jsonPropertyName)
        {
            return GetType().GetProperties()?.FirstOrDefault(p =>
                p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName?.Equals(jsonPropertyName) ?? false);
        }
    }
}