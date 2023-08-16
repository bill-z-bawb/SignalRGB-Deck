using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewlayouts")]
    public class SignalRgbLayoutsView : SignalRgbViewActionBase
    {
        public SignalRgbLayoutsView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/layouts";
    }
}