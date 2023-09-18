using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    internal static class LayoutsHelper
    {
        private const string SignalRgbLayoutsKey = @"Software\WhirlwindFX\SignalRgb\layouts";

        internal static Dictionary<string, UserLayout> LayoutsDatabase = new Dictionary<string, UserLayout>();
        internal static List<UserLayout> Layouts => LayoutsDatabase.Select(kv => kv.Value).ToList();

        internal static void RefreshLayoutsDatabase()
        {
            var streamDeckAppPath = Registry.CurrentUser.OpenSubKey(SignalRgbLayoutsKey);
            if (!streamDeckAppPath?.GetSubKeyNames()?.Any() ?? true)
            {
                LayoutsDatabase = new Dictionary<string, UserLayout>();
                return;
            }

            LayoutsDatabase = streamDeckAppPath
                .GetSubKeyNames()
                .Select(k => new KeyValuePair<string, UserLayout>(k, new UserLayout(k,k)))
                .ToDictionary(l => l.Key, l => l.Value);
        }

        public static UserLayout LayoutLookup(string forLayoutId)
        {
            if (string.IsNullOrWhiteSpace(forLayoutId))
                return null;
            
            if (!LayoutsDatabase?.Any() ?? true)
            {
                RefreshLayoutsDatabase();
            }

            return LayoutsDatabase[forLayoutId];
        }
    }
}