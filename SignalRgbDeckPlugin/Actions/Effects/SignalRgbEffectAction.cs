using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    [PluginActionId("com.billzbawb.signalrgb.effect")]
    public class SignalRgbEffectAction : SignalRgbKeypadBase
    {
        #region Constants and Private Members

        public const string EffectPropMarker = "-effect-prop";
        private readonly EffectActionSettings settings;
        
        #endregion

        #region Construction / Destruction
        public SignalRgbEffectAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            GlobalSettingsManager.Instance.RequestGlobalSettings();
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = EffectActionSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<EffectActionSettings>();
            }

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

        private void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.SendToPlugin> e)
        {
            var payload = e.Event.Payload;
            if (payload["property_inspector"] != null)
            {
                switch (payload["property_inspector"].ToString())
                {
                    case "fetchEffectProps":
                        var forEffectId = payload["effectId"]?.ToString();
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"fetchEffectProps called for effect ID {forEffectId}");
                        settings.SelectedEffectId = forEffectId;
                        settings.SelectedEffect = EffectsHelper.EffectLookup(forEffectId);
                        SaveSettings();
                        break;
                }
            }
        }

        private void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidAppear> e)
        {
            EffectsHelper.RefreshEffectsDatabase();
            settings.InstalledEffects = EffectsHelper.EffectSummaries;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Sent {settings.InstalledEffects.Count} effects to PI...");
            SaveSettings();
        }

        private void Connection_OnTitleParametersDidChange(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.TitleParametersDidChange> e)
        {
            CurrentTitleParameters = e.Event.Payload.TitleParameters;
            UpdateEffectButtonTitle();
        }

        private void Connection_OnPropertyInspectorDidDisappear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidDisappear> e)
        {

        }
        
        private void Connection_OnDeviceDidDisconnect(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.DeviceDidDisconnect> e)
        {
        }

        private void Connection_OnDeviceDidConnect(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.DeviceDidConnect> e)
        {
            
        }

        private void Connection_OnApplicationDidTerminate(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.ApplicationDidTerminate> e)
        {
        }

        private void Connection_OnApplicationDidLaunch(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.ApplicationDidLaunch> e)
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
            // standard PI field handling
            Tools.AutoPopulateSettings(settings, payload.Settings);
            
            // custom effects-based (dynamic) settings handling
            settings.SelectedEffect = EffectsHelper.EffectLookup(settings.SelectedEffectId);
            foreach (var prop in payload.Settings)
            {
                if (!prop.Key.Contains(EffectPropMarker))
                    continue;

                var propKey = prop.Key.Replace(EffectPropMarker, string.Empty);
                settings.SelectedEffect.SetPropertyValue(propKey, prop.Value.ToString());
            }

            UpdateEffectButtonTitle();
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {

        }

        #endregion

        #region Private Methods

        protected override void UpdateEffectButtonTitle()
        {
            if (settings?.SelectedEffect != null)
                SetEffectButtonTitle(settings.SelectedEffect.Name);
        }

        protected override Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion

        #region SignalRGB Implementation

        public override string[] ApplicationUrls
        {
            get
            {
                var url = new StringBuilder();
                url.Append("signalrgb://effect/apply/");

                // add the effect's name
                url.Append(Uri.EscapeDataString(settings.SelectedEffect.Name));

                // add the effect's settings
                url.Append(settings.SelectedEffect.PropsAsApplicationUrlArgString(true));

                return new []{ url.ToString() };
            }
        }

        #endregion
    }
}