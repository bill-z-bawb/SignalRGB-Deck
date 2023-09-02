// See https://aka.ms/new-console-template for more information
using StreamDeckDeployer;

PluginDeploymentHelper.RenderTitle();

try
{
    if (PluginDeploymentHelper.HasArgKey(PluginReleaseTrigger.TriggerReleaseKey, args))
    {
        PluginReleaseTrigger.Start(args);
        return;
    }

    PluginBuildTools.Start(args);
}
finally
{
    Console.ResetColor();
    PluginDeploymentHelper.RenderHr();
    Environment.Exit(0);
}
