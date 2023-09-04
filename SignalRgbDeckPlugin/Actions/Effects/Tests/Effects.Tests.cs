using System.IO;
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
            Assert.Contains("Side to Side", prop.Values);
            Assert.Contains("All Directions", prop.Values);
            Assert.Contains("Up and Down", prop.Values);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("color1"));
            Assert.NotNull(prop);
            Assert.Equal("color", prop.Type);
            Assert.Equal("Color 1", prop.Label);
            Assert.Equal("#F55200", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(360, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("color2"));
            Assert.NotNull(prop);
            Assert.Equal("color", prop.Type);
            Assert.Equal("Color 2", prop.Label);
            Assert.Equal("#00F5D3", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(360, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("rainbow"));
            Assert.NotNull(prop);
            Assert.Equal("boolean", prop.Type);
            Assert.Equal("Rainbow Mode", prop.Label);
            Assert.Equal("0", prop.Default);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("speed"));
            Assert.NotNull(prop);
            Assert.Equal("number", prop.Type);
            Assert.Equal("Speed", prop.Label);
            Assert.Equal("2", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(100, prop.Max);
        }

        [Fact]
        public void can_parse_effect_that_lists_prop_attributes_on_newlines()
        {
            var effectFile = EffectsTestsHelper.OtherEffectsFiles.First(f => f.Name.Equals("effect-props-on-newlines.html"));
            var parsed = InstalledEffectDetail.EffectFromHtml(effectFile);
            Assert.NotNull(parsed);
            Assert.Equal("Visor", parsed.Name);
            Assert.Equal(6, parsed.Properties.Count);

            var prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("color"));
            Assert.NotNull(prop);
            Assert.Equal("color", prop.Type);
            Assert.Equal("Color", prop.Label);
            Assert.Equal("#ff0066", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(360, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("randomColors"));
            Assert.NotNull(prop);
            Assert.Equal("boolean", prop.Type);
            Assert.Equal("Random Color", prop.Label);
            Assert.Equal("0", prop.Default);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("speed"));
            Assert.NotNull(prop);
            Assert.Equal("number", prop.Type);
            Assert.Equal("Visor Speed", prop.Label);
            Assert.Equal("30", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(100, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("barWidth"));
            Assert.NotNull(prop);
            Assert.Equal("number", prop.Type);
            Assert.Equal("Bar Width", prop.Label);
            Assert.Equal("20", prop.Default);
            Assert.Equal(1, prop.Min);
            Assert.Equal(50, prop.Max);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("vertical"));
            Assert.NotNull(prop);
            Assert.Equal("boolean", prop.Type);
            Assert.Equal("Vertical", prop.Label);
            Assert.Equal("1", prop.Default);

            prop = parsed.Properties.FirstOrDefault(p => p.PropertyName.Equals("bgColor"));
            Assert.NotNull(prop);
            Assert.Equal("color", prop.Type);
            Assert.Equal("Background Color", prop.Label);
            Assert.Equal("#000000", prop.Default);
            Assert.Equal(0, prop.Min);
            Assert.Equal(360, prop.Max);
        }

        [Fact]
        public void can_parse_cached_folder()
        {
            // Effect "Visor"
            var effectFolder = EffectsTestsHelper.CachedEffectsFolders.First(f => f.Name.Equals("-M6IRGbcnL4HQaVooCgs"));
            var parsed = InstalledEffectDetail.EffectFromCacheDirectory(effectFolder);

            Assert.NotNull(parsed);
            Assert.Equal("Visor", parsed.Name);
            Assert.Equal("-M6IRGbcnL4HQaVooCgs", parsed.Id);
            Assert.Equal(6, parsed.Properties.Count);
        }

        [Fact]
        public void throws_on_empty_cache_folder()
        {
            Assert.Throws<FileNotFoundException>(() => InstalledEffectDetail.EffectFromCacheDirectory(EffectsTestsHelper.CachedEffectsFolder));
        }

        [Fact]
        public void identifies_empty_cache_folder()
        {
            Assert.False(InstalledEffectDetail.EffectsCacheDirectoryHasEffect(EffectsTestsHelper.CachedEffectsFolder));
        }

        [Fact]
        public void identifies_cache_folder_has_effect()
        {
            var effectFolder = EffectsTestsHelper.CachedEffectsFolders.First(f => f.Name.Equals("-M6IRGbcnL4HQaVooCgs"));
            Assert.True(InstalledEffectDetail.EffectsCacheDirectoryHasEffect(effectFolder));
        }

        [Fact]
        public void effect_details_can_cast_to_effect()
        {
            var effectFile = EffectsTestsHelper.InstalledEffectsFiles.First(f => f.Name.Equals("Side To Side.html"));
            var parsed = InstalledEffectDetail.EffectFromHtml(effectFile);
            var cast = parsed.ToInstalledEffect();

            Assert.NotNull(cast);
            Assert.Equal("Side to Side", cast.Name);
            Assert.NotNull(cast.Id);
            Assert.NotEqual(string.Empty, cast.Id);
        }
    }
}