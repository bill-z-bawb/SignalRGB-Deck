using System.Text;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.EffectAndLayoutCombo
{
    [PluginActionId("com.billzbawb.signalrgb.previouseffectandlayout")]
    public class SignalRgbPreviousEffectAndLayoutAction : SignalRgbKeypadBase
    {
        public override string[] ApplicationUrls
        {
            get
            {
                var effectUrl = new StringBuilder();
                effectUrl.Append("signalrgb://effect/applyprevious");
                effectUrl.Append($"?{SilentLaunchRequest}");

                var layoutUrl = new StringBuilder();
                layoutUrl.Append("signalrgb://layout/applyprevious");
                layoutUrl.Append($"?{SilentLaunchRequest}");

                return new[] { effectUrl.ToString(), layoutUrl.ToString() };
            }
        }

        public SignalRgbPreviousEffectAndLayoutAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }
    }
}