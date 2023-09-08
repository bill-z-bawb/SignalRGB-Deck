using System.Text;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    [PluginActionId("com.billzbawb.signalrgb.previouseffect")]
    public class SignalRgbPreviousEffectAction : SignalRgbKeypadBase
    {
        public override bool IsApplicationUrlSetValid => true;

        public override string[] ApplicationUrls
        {
            get
            {
                var url = new StringBuilder();
                url.Append("signalrgb://effect/applyprevious");

                // direction for silent launch
                url.Append($"?{SilentLaunchRequest}");

                return new []{ url.ToString() };
            }
        }

        public SignalRgbPreviousEffectAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }
    }
}