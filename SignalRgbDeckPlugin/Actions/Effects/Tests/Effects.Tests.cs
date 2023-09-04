using System.Linq;
using Xunit;

namespace SignalRgbDeckPlugin.Actions.Effects.Tests
{
    public class EffectsTests
    {
        [Fact]
        public void can_parse_effect()
        {
            var effectFile = EffectsTestsHelper.InstalledEffectsFiles.First(f => f.Name.Equals("Side To Side.html"));
            var parsed = InstalledEffectDetail.EffectFromHtml(effectFile);
            Assert.NotNull(parsed);
            Assert.Equal("Side to Side", parsed.Name);
            Assert.Equal(5, parsed.Properties.Count);

            var prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("direction"));
            Assert.NotNull(prop);
            Assert.Equal("combobox", prop.Type);
            Assert.Equal("Direction", prop.Label);
            Assert.Equal("All Directions", prop.Default);
            Assert.Equal(3, prop.Values.Count);
            Assert.True(prop.Values.Contains("Side to Side"));
            Assert.True(prop.Values.Contains("All Directions"));
            Assert.True(prop.Values.Contains("Up and Down"));

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("color1"));
            Assert.NotNull(prop);
            Assert.Equal("color", prop.Type);
            Assert.Equal("Color 1", prop.Label);
            Assert.Equal("#F55200", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(360, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("color2"));
            Assert.NotNull(prop);
            Assert.NotNull(prop);
            Assert.Equal("color", prop.Type);
            Assert.Equal("Color 2", prop.Label);
            Assert.Equal("#00F5D3", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(360, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("rainbow"));
            Assert.NotNull(prop);
            Assert.NotNull(prop);
            Assert.Equal("boolean", prop.Type);
            Assert.Equal("Rainbow Mode", prop.Label);
            Assert.Equal("0", prop.Default);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("speed"));
            Assert.NotNull(prop);
            Assert.NotNull(prop);
            Assert.NotNull(prop);
            Assert.Equal("number", prop.Type);
            Assert.Equal("Speed", prop.Label);
            Assert.Equal("2", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(100, prop.Max);
        }
    }
}