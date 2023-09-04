using System.IO;
using System.Runtime.CompilerServices;

internal static class ProjectSourcePath
{
    private const string RelativePath = nameof(ProjectSourcePath) + ".cs";
    private static string _lazyValue;
    public static string Value => _lazyValue ?? CalculatePath();
    public static string ProjectPath => string.IsNullOrWhiteSpace(Value) ? null : Path.GetFullPath(Path.Combine(Value, "..\\"));

    private static string CalculatePath()
    {
        var pathName = GetSourceFilePathName();
        return pathName.Substring(0, pathName.Length - RelativePath.Length);
    }

    public static string GetSourceFilePathName([CallerFilePath] string callerFilePath = null) => callerFilePath ?? "";
}