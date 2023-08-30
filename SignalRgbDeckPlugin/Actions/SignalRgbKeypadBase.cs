using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalRgbDeckPlugin.Actions
{
    public abstract class SignalRgbKeypadBase : KeypadBase
    {
        public const string SilentLaunchRequest = "-silentlaunch-";

        public abstract bool IsApplicationUrlValid { get; }
        public abstract string ApplicationUrl { get; }

        #region Private Members

        protected TitleParameters _currenTitleParameters = null;

        #endregion

        #region Construction / Destruction
        protected SignalRgbKeypadBase(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            GlobalSettingsManager.Instance.RequestGlobalSettings();

            Connection.OnApplicationDidLaunch += Connection_OnApplicationDidLaunch;
            Connection.OnApplicationDidTerminate += Connection_OnApplicationDidTerminate;
            Connection.OnDeviceDidConnect += Connection_OnDeviceDidConnect;
            Connection.OnDeviceDidDisconnect += Connection_OnDeviceDidDisconnect;
            Connection.OnPropertyInspectorDidAppear += Connection_OnPropertyInspectorDidAppear;
            Connection.OnPropertyInspectorDidDisappear += Connection_OnPropertyInspectorDidDisappear;
            Connection.OnSendToPlugin += Connection_OnSendToPlugin;
            Connection.OnTitleParametersDidChange += Connection_OnTitleParametersDidChange;
        }

        public override void Dispose()
        {
            Connection.OnApplicationDidLaunch -= Connection_OnApplicationDidLaunch;
            Connection.OnApplicationDidTerminate -= Connection_OnApplicationDidTerminate;
            Connection.OnDeviceDidConnect -= Connection_OnDeviceDidConnect;
            Connection.OnDeviceDidDisconnect -= Connection_OnDeviceDidDisconnect;
            Connection.OnPropertyInspectorDidAppear -= Connection_OnPropertyInspectorDidAppear;
            Connection.OnPropertyInspectorDidDisappear -= Connection_OnPropertyInspectorDidDisappear;
            Connection.OnSendToPlugin -= Connection_OnSendToPlugin;
            Connection.OnTitleParametersDidChange -= Connection_OnTitleParametersDidChange;
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        #endregion

        #region Handlers

        protected virtual void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.SendToPlugin> e)
        {
        }

        protected virtual void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidAppear> e)
        {
            SaveSettings();
        }

        protected virtual void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            _currenTitleParameters = e.Event.Payload.TitleParameters;
            UpdateEffectButtonTitle();
        }

        protected virtual void Connection_OnPropertyInspectorDidDisappear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidDisappear> e)
        {
        }

        protected virtual void Connection_OnDeviceDidDisconnect(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.DeviceDidDisconnect> e)
        {
        }

        protected virtual void Connection_OnDeviceDidConnect(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.DeviceDidConnect> e)
        {
        }

        protected virtual void Connection_OnApplicationDidTerminate(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.ApplicationDidTerminate> e)
        {
        }

        protected virtual void Connection_OnApplicationDidLaunch(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.ApplicationDidLaunch> e)
        {
        }

        #endregion

        #region Action Behavior

        public override void KeyPressed(KeyPayload payload)
        {
        }

        public override void OnTick()
        {
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (IsApplicationUrlValid)
                OpenWebsite(ApplicationUrl);
        }

        private static void OpenWebsite(string url) =>
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

        #endregion

        #region Private Methods

        protected virtual void UpdateEffectButtonTitle()
        {

        }

        protected virtual void SetEffectButtonTitle(string title)
        {
            Connection.SetTitleAsync(title.SplitToFitKey(_currenTitleParameters));
        }

        protected virtual Task SaveSettings()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}