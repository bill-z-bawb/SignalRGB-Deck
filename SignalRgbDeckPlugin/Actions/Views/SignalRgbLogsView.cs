using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewlogs")]
    public class SignalRgbLogsView : SignalRgbViewActionBase
    {
        public SignalRgbLogsView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/logs";
    }
}