using System;
using System.Text;
using System.Threading.Tasks;
using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json.Linq;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    [PluginActionId("com.billzbawb.signalrgb.layout")]
    public class SignalRgbLayoutAction : SignalRgbKeypadBase
    {
        #region Private Members

        private readonly LayoutActionSettings settings;
        
        #endregion

        #region Construction / Destruction
        public SignalRgbLayoutAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = LayoutActionSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<LayoutActionSettings>();
            }
        }
        
        #endregion

        #region Handlers
        
        protected override void Connection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<BarRaider.SdTools.Events.PropertyInspectorDidAppear> e)
        {
            LayoutsHelper.RefreshLayoutsDatabase();
            settings.UserLayouts = LayoutsHelper.Layouts;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Sent {settings.UserLayouts.Count} layouts to PI...");
            SaveSettings();
        }

        #endregion

        #region Action Behavior

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            // standard PI field handling
            Tools.AutoPopulateSettings(settings, payload.Settings);

            settings.SelectedLayout = LayoutsHelper.LayoutLookup(settings.SelectedLayoutId);
            
            UpdateEffectButtonTitle();
            SaveSettings();
        }

        #endregion

        #region Private Methods

        protected override void UpdateEffectButtonTitle()
        {
            if (settings?.SelectedLayout != null)
                SetEffectButtonTitle(settings.SelectedLayout.Name);
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
                url.Append("signalrgb://layout/apply/");

                // add the layout's name
                url.Append(Uri.EscapeDataString(settings.SelectedLayout.Name));
                
                // direction for silent launch
                url.Append($"?{SilentLaunchRequest}");

                return new[] { url.ToString() };
            }
        }

        #endregion
    }
}