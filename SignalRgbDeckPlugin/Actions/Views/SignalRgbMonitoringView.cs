using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewmonitoring")]
    public class SignalRgbMonitoringView : SignalRgbViewActionBase
    {
        public SignalRgbMonitoringView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string ApplicationUrl => "signalrgb://view/monitoring";
    }
}