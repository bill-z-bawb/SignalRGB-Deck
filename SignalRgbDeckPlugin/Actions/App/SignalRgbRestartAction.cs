using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.App
{
    [PluginActionId("com.billzbawb.signalrgb.apprestart")]
    public class SignalRgbRestartAction : SignalRgbKeypadBase
    {
        public override string[] ApplicationUrls => new[] { "signalrgb://app/restart" };

        public SignalRgbRestartAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }
    }
}