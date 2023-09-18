using System.Text;
using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    [PluginActionId("com.billzbawb.signalrgb.previouslayout")]
    public class SignalRgbPreviousLayoutAction : SignalRgbKeypadBase
    {
        public override string[] ApplicationUrls
        {
            get
            {
                var url = new StringBuilder();
                url.Append("signalrgb://layout/applyprevious");

                // direction for silent launch
                url.Append($"?{SilentLaunchRequest}");

                return new[] { url.ToString() };
            }
        }

        public SignalRgbPreviousLayoutAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }
    }
}