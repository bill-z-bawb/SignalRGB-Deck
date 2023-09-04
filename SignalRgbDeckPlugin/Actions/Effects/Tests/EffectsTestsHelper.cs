using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SignalRgbDeckPlugin.Actions.Effects.Tests
{
    internal static class EffectsTestsHelper
    {
        public static string TestResourcesPath =>
            Path.Combine(ProjectSourcePath.ProjectPath, @"Actions\Effects\Tests\TestResources\");
        public static DirectoryInfo TestResourcesFolder =>
            new DirectoryInfo(TestResourcesPath);

        public static string CachedEffectsPath =>
            Path.Combine(TestResourcesPath, @"CachedEffects");
        public static DirectoryInfo CachedEffectsFolder =>
            new DirectoryInfo(CachedEffectsPath);

        public static string InstalledEffectsPath =>
            Path.Combine(TestResourcesPath, @"InstalledEffects");
        public static DirectoryInfo InstalledEffectsFolder =>
            new DirectoryInfo(InstalledEffectsPath);

        public static List<DirectoryInfo> CachedEffectsFolders => 
            CachedEffectsFolder.GetDirectories("*", SearchOption.TopDirectoryOnly).ToList();

        public static List<FileInfo> InstalledEffectsFiles =>
            InstalledEffectsFolder.GetFiles("*.html", SearchOption.AllDirectories).ToList();
    }
}
