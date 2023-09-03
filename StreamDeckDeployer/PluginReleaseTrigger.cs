using System.Diagnostics;
using System.Drawing;
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
                    $"\n\nCreated tag \"{releaseTagRef.Ref}\" at commit \"{releaseTagRef.Object.Sha}\", GHA should be running now, would you like to monitor this release run? [y / n]: ");

                var openGhActions = Console.ReadKey();
                if (openGhActions.Key != ConsoleKey.Y)
                    return;

                MonitorReleaseAction(nextVersionStr, client);
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

        public static void MonitorReleaseAction(string releaseToMonitor, GitHubClient? client = null)
        {
            if (client == null)
            {
                DotNetEnv.Env.TraversePath().Load();
                client = GetClient(DotNetEnv.Env.GetString(EnvKeyGitHubPat));
            }
            
            Console.WriteLine($"Searching for GitHub Release Run {releaseToMonitor}...");
            var restartPos = new Point(Console.CursorLeft, Console.CursorTop);
            Console.Write("Waiting");
            var waitingDotsPos = new Point(Console.CursorLeft, Console.CursorTop);
            var waitCtr = 0;

            var sw = Stopwatch.StartNew();
            WorkflowRun thisRun = null;
            while (sw.Elapsed.TotalSeconds < 180)
            {
                var w = client.Actions.Workflows.Runs.ListByWorkflow(GitRepoOwner, GitRepoName, "build-and-release.yml").Result;
                if (w == null) continue;
                thisRun = w.WorkflowRuns.FirstOrDefault(r => r.HeadBranch.Equals(releaseToMonitor));
                if (thisRun == null)
                {
                    Thread.Sleep(1000);
                    RenderWaiting(waitingDotsPos, waitCtr++);
                    continue;
                }

                Console.SetCursorPosition(restartPos.X, restartPos.Y);
                break;
            }

            if (thisRun == null)
            {
                throw new Exception($"Unable to find the workflow run for release {releaseToMonitor}");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Monitoring Release Workflow Run for {releaseToMonitor} (created: {thisRun.CreatedAt})...");
            Console.ResetColor();

            restartPos = new Point(Console.CursorLeft, Console.CursorTop);
            
            WorkflowRunStatus? lastRunStatus = null;
            while (thisRun.Status.Value != WorkflowRunStatus.Completed)
            {
                if (lastRunStatus == thisRun.Status.Value)
                {
                    RenderWaiting(waitingDotsPos, waitCtr++);
                    Thread.Sleep(5000);
                    thisRun = client.Actions.Workflows.Runs.Get(GitRepoOwner, GitRepoName, thisRun.Id).Result;
                    continue;
                }

                lastRunStatus = thisRun.Status.Value;

                Console.SetCursorPosition(restartPos.X, restartPos.Y);
                Console.Write($"Release workflow status update => {thisRun.Status.StringValue}...");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(thisRun.Status.StringValue);
                Console.ResetColor();
                waitCtr = 0;
                restartPos = new Point(Console.CursorLeft, Console.CursorTop);
                Console.Write("Checking");
                waitingDotsPos = new Point(Console.CursorLeft, Console.CursorTop);
            }

            Console.SetCursorPosition(restartPos.X, restartPos.Y);
            Console.ForegroundColor = GetConclusionColor(thisRun);
            Console.WriteLine($"Release workflow is completed with status: {thisRun.Conclusion}");
            Console.ResetColor();
            Console.Write($"\nOpen the run on GitHub [y/n]?: ");

            var openGhActions = Console.ReadKey();

            if (openGhActions.Key is ConsoleKey.Y)
            {
                Helper.OpenWebsite(thisRun.HtmlUrl);
            }
        }

        private const int WaitingDots = 5;
        private static void RenderWaiting(Point waitingPos, int waitCount)
        {
            var numDots = (waitCount % WaitingDots) + 1;
            Console.SetCursorPosition(waitingPos.X, waitingPos.Y);
            Console.Write("".PadRight(WaitingDots, ' '));
            Console.SetCursorPosition(waitingPos.X, waitingPos.Y);
            Console.Write("".PadRight(numDots, '.'));
        }

        private static ConsoleColor GetConclusionColor(WorkflowRun run)
        {
            if (!run.Conclusion.HasValue)
                throw new Exception("Not concluded!");

            if (run.Conclusion == WorkflowRunConclusion.ActionRequired ||
                run.Conclusion == WorkflowRunConclusion.Cancelled ||
                run.Conclusion == WorkflowRunConclusion.Stale ||
                run.Conclusion == WorkflowRunConclusion.Neutral)
            {
                return ConsoleColor.Yellow;
            }

            if (run.Conclusion == WorkflowRunConclusion.Failure ||
                run.Conclusion == WorkflowRunConclusion.StartupFailure ||
                run.Conclusion == WorkflowRunConclusion.Skipped ||
                run.Conclusion == WorkflowRunConclusion.TimedOut)
            {
                return ConsoleColor.Red;
            }

            if (run.Conclusion == WorkflowRunConclusion.Success)
            {
                return ConsoleColor.Green;
            }

            return ConsoleColor.White;
        }
    }
}
