using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewplugins")]
    public class SignalRgbPluginsView : SignalRgbViewActionBase
    {
        public SignalRgbPluginsView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/userplugins";
    }
}