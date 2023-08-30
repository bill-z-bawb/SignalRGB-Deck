using BarRaider.SdTools;
using System.Diagnostics;

namespace SignalRgbDeckPlugin.Actions
{
    public abstract class SignalRgbKeypadBase : KeypadBase
    {
        public const string SilentLaunchRequest = "-silentlaunch-";

        protected SignalRgbKeypadBase(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public abstract bool IsApplicationUrlValid { get; }
        public abstract string ApplicationUrl { get; }

        public override void KeyReleased(KeyPayload payload)
        {
            if (IsApplicationUrlValid)
                OpenWebsite(ApplicationUrl);
        }

        private static void OpenWebsite(string url) => 
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}