using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.App
{
    [PluginActionId("com.billzbawb.signalrgb.apprestart")]
    public class SignalRgbRestartAction : SignalRgbKeypadBase
    {
        public override bool IsApplicationUrlValid => true;
        public override string ApplicationUrl => "signalrgb://app/restart";

        public SignalRgbRestartAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }
    }
}