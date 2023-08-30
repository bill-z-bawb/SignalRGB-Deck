using System.Text;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    [PluginActionId("com.billzbawb.signalrgb.previouslayout")]
    public class SignalRgbPreviousLayoutAction : SignalRgbKeypadBase
    {
        public override bool IsApplicationUrlValid => true;

        public override string ApplicationUrl
        {
            get
            {
                var url = new StringBuilder();
                url.Append("signalrgb://layout/applyprevious");

                // direction for silent launch
                url.Append($"?{SilentLaunchRequest}");

                return url.ToString();
            }
        }

        public SignalRgbPreviousLayoutAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }
    }
}