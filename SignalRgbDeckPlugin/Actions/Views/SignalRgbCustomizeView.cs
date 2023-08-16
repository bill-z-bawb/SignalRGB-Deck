using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewcustomize")]
    public class SignalRgbCustomizeView : SignalRgbViewActionBase
    {
        public SignalRgbCustomizeView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/customize";
    }
}