using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewdashboard")]
    public class SignalRgbDashboardView : SignalRgbViewActionBase
    {
        public SignalRgbDashboardView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string[] ApplicationUrls => new[] { "signalrgb://view/dashboard" };
    }
}