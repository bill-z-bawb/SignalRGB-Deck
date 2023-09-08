using BarRaider.SdTools;

namespace SignalRgbDeckPlugin.Actions.Views
{
    public abstract class SignalRgbViewActionBase : SignalRgbKeypadBase
    {
        public override bool IsApplicationUrlSetValid => true;

        protected SignalRgbViewActionBase(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override void KeyPressed(KeyPayload payload)
        {

        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {

        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {

        }

        public override void OnTick()
        {

        }

        public override void Dispose()
        {

        }
    }
}