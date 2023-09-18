using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    [PluginActionId("com.billzbawb.signalrgb.viewcooling")]
    public class SignalRgbCoolingView : SignalRgbViewActionBase
    {
        public SignalRgbCoolingView(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override string[] ApplicationUrls => new []{ "signalrgb://view/cooling" };
    }
}