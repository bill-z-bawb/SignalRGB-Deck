using Newtonsoft.Json;
using Octokit;
using Helper = StreamDeckDeployer.PluginDeploymentHelper;

namespace StreamDeckDeployer
{
    internal static class PluginReleaseTrigger
    {
        public const string TriggerReleaseKey = "triggerRelease";
        private const string PublishReleaseBranchKey = "publishReleaseBranch";

        private const string GitRepoOwner = "bill-z-bawb";
        private const string GitRepoName = "SignalRGB-Deck";

        private const string EnvKeyGitHubPat = "GITHUB_PAT";
        private const string EnvKeyPluginRoot = "PLUGIN_ROOT";

        private const string ReleaseActionUrl = "https://github.com/bill-z-bawb/SignalRGB-Deck/actions";

        public static void Start(string[] args)
        {
            var branch = Helper.GetArgValue(PublishReleaseBranchKey, args);
            branch = !string.IsNullOrWhiteSpace(branch) ? branch : "master";
            Console.WriteLine($"Tag and start GHA release publish on \"{branch}\"...");

            DotNetEnv.Env.TraversePath().Load();

            try
            {
                Validate(out var pluginDir, out var gitHubPat);

                var client = GetClient(gitHubPat);
                GetPluginProps(pluginDir, out var pluginName, out var nextVersion);
                var nextVersionStr = $"v{nextVersion.ToString(3)}";
                var branchSha = ShaForBranch(client, branch);

                Console.WriteLine($"You are about to release {pluginName} {nextVersionStr}...");
                Console.WriteLine($"Continuing will tag \"{branch}\" branch with SHA {branchSha} for release...\n");
                Console.Write("Continue? [y / n]: ");
                var continueRelease = Console.ReadKey();

                if (continueRelease.Key != ConsoleKey.Y)
                    return;

                var releaseTagRef = MakeVersionTag(client, nextVersionStr, branchSha);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(
                    $"\n\nCreated tag \"{releaseTagRef.Ref}\" at commit \"{releaseTagRef.Object.Sha}\", GHA should be running now, would you like to go there and see? [y / n]: ");

                var openGhActions = Console.ReadKey();
                if (openGhActions.Key != ConsoleKey.Y)
                    return;

                Helper.OpenWebsite(ReleaseActionUrl);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\n\nOpened URL: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(ReleaseActionUrl);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed! Unable to start release! {ex.Message}");
                Console.ResetColor();
            }
        }

        private static Reference MakeVersionTag(GitHubClient client, string versionText, string branchSha)
        {
            try
            {
                var releaseTagNewRef = new NewReference($"refs/tags/{versionText}", branchSha);
                return client.Git.Reference.Create(GitRepoOwner, GitRepoName, releaseTagNewRef).Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to make version tag for \"{versionText}\"! {ex.Message}", ex);
            }
        }

        private static void Validate(out DirectoryInfo pluginDir, out string gitHubPat)
        {
            var targetPluginPath = DotNetEnv.Env.GetString(EnvKeyPluginRoot);
            if (string.IsNullOrWhiteSpace(targetPluginPath))
                throw new Exception(
                    $"Error! The target plugin (what you are releasing is not defined! (specify \"{EnvKeyGitHubPat}\" as absolute path to plugin in the .env)");

            pluginDir = new DirectoryInfo(targetPluginPath);
            if (!pluginDir.Exists)
                throw new Exception(
                    $"Error! The target plugin at \"{targetPluginPath}\" does not exist!");

            gitHubPat = DotNetEnv.Env.GetString(EnvKeyGitHubPat);
            if (string.IsNullOrWhiteSpace(gitHubPat))
                throw new Exception(
                    $"Error! A GitHub Personal Access Token must be provided in the .env file! (named \"{EnvKeyGitHubPat}\")");
        }

        private static GitHubClient GetClient(string gitHubPat)
        {
            return new GitHubClient(new ProductHeaderValue("StreamDeckDeployer"))
            {
                Credentials = new Credentials(gitHubPat),
            };
        }

        // Helpers
        private static string ShaForBranch(GitHubClient client, string branch)
        {
            var b = client.Repository.Branch.Get(GitRepoOwner, GitRepoName, branch).Result;
            return b == null
                ? throw new Exception($"Error! Branch \"{branch}\" could not be found.")
                : b.Commit.Sha;
        }

        private static void GetPluginProps(DirectoryInfo pluginPath, out string name, out Version nextReleaseVer)
        {
            try
            {
                var manifest = pluginPath.GetFiles("*.json", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault(f => f.Name.Equals("manifest.json"));

                if (manifest == null)
                    throw new Exception($"The plugin project at \"{pluginPath.FullName}\" has no \"manifest.json\" file!");

                var manifestObj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(manifest.FullName));

                if (manifestObj.Name == null)
                {
                    throw new Exception(
                        $"The plugin project at \"{pluginPath.FullName}\" has a \"manifest.json\" file, but the \"Name\" field is missing!");
                }
                name = manifestObj.Name.Value;

                if (manifestObj.Version == null)
                {
                    throw new Exception(
                        $"The plugin project at \"{pluginPath.FullName}\" has a \"manifest.json\" file, but the \"Version\" field is missing!");
                }
                nextReleaseVer = new Version(manifestObj.Version.Value as string);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to derive next version! {ex.Message}", ex);
            }
        }
    }
}
