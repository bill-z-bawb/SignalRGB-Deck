using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewdumps")]
    public class SignalRgbDumpsView : SignalRgbViewActionBase
    {
        public SignalRgbDumpsView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string[] ApplicationUrls => new[] { "signalrgb://view/dumps" };
    }
}