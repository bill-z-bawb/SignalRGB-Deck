using System.Collections.Generic;

namespace SignalRgbDeckPlugin.Actions.Layouts
{
    public interface ILayoutActionSettings
    {
        string SelectedLayoutId { get; set; }
        UserLayout SelectedLayout { get; set; }
        List<UserLayout> UserLayouts { get; set; }
    }
}