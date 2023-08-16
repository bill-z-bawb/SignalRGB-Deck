using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewdebuginfo")]
    public class SignalRgbDebugInfoView : SignalRgbViewActionBase
    {
        public SignalRgbDebugInfoView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/debuginfo";
    }
}