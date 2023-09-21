using System.Collections.Generic;

namespace SignalRgbDeckPlugin.Actions.Effects
{
    public interface IEffectActionSettings
    {
        string SelectedEffectPreset { get; set; }
        string SelectedEffectId { get; set; }
        InstalledEffectDetail SelectedEffect { get; set; }
        List<InstalledEffect> InstalledEffects { get; set; }
    }
}