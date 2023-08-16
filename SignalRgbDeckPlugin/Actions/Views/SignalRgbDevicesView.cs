using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewdevices")]
    public class SignalRgbDevicesView : SignalRgbViewActionBase
    {
        public SignalRgbDevicesView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/devices";
    }
}