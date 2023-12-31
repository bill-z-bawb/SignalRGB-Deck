﻿using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRgbDeckPlugin.Actions
{
    public abstract class SignalRgbKeypadBase : KeypadBase
    {
        public const string SilentLaunchRequest = "-silentlaunch-";

        public abstract string[] ApplicationUrls { get; }

        #region Private Members

        protected TitleParameters CurrentTitleParameters = null;

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
            CurrentTitleParameters = e.Event.Payload.TitleParameters;
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
            foreach (var applicationUrl in ApplicationUrls)
            {
                try
                {
                    OpenWebsite(applicationUrl);
                    // jank AF, but there is no way to verify if a command has been process and if given too many command near the same time
                    // only the "last" will be processed.
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"{nameof(SignalRgbKeypadBase)} => {GetType()} KeyReleased failed for url \"{applicationUrl}\"! {ex.Message}");
                }
            }
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
            Connection.SetTitleAsync(title.SplitToFitKey(CurrentTitleParameters));
        }

        protected virtual Task SaveSettings()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}