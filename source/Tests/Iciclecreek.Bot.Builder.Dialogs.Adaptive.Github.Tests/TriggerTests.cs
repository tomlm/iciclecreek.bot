using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub;
using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub.Triggers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Memory;
using System.Diagnostics;

namespace Iciclecreek.Bot.Builder.Dialogs.Adaptive.Github.Tests
{
    [TestClass]
    public class TriggerTests
    {
        private static OnCondition AddActions(OnCondition trigger)
        {
            trigger.Actions = new List<Dialog>()
            {
                new SendActivity(trigger.GetType().Name)
            };
            return trigger;
        }

        public static Dialog dialog;
        public static GitHubAdapter adapter;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            dialog = new AdaptiveDialog()
            {
                Recognizer = new RegexRecognizer(),
                Triggers = new List<OnCondition>()
                    {
                        AddActions(new OnGitHubCheckRunEvent()),
                        AddActions(new OnGitHubCheckSuiteEvent()),
                        AddActions(new OnGitHubCodeScanningAlertEvent()),
                        AddActions(new OnGitHubCommitCommentEvent()),
                        AddActions(new OnGitHubContentReferenceEvent()),
                        AddActions(new OnGitHubCreateEvent()),
                        AddActions(new OnGitHubDeleteEvent()),
                        AddActions(new OnGitHubDeployKeyEvent()),
                        AddActions(new OnGitHubDeploymentEvent()),
                        AddActions(new OnGitHubDeploymentStatusEvent()),
                        AddActions(new OnGitHubForkEvent()),
                        AddActions(new OnGitHubGithubAppAuthorizationEvent()),
                        AddActions(new OnGitHubGollumEvent()),
                        AddActions(new OnGitHubInstallationEvent()),
                        AddActions(new OnGitHubInstallationRepositoriesEvent()),
                        AddActions(new OnGitHubIssueCommentEvent()),
                        AddActions(new OnGitHubIssuesEvent()),
                        AddActions(new OnGitHubLabelEvent()),
                        AddActions(new OnGitHubMarketplacePurchaseEvent()),
                        AddActions(new OnGitHubMemberEvent()),
                        AddActions(new OnGitHubMembershipEvent()),
                        AddActions(new OnGitHubMetaEvent()),
                        AddActions(new OnGitHubMilestoneEvent()),
                        AddActions(new OnGitHubOrganizationEvent()),
                        AddActions(new OnGitHubOrgBlockEvent()),
                        AddActions(new OnGitHubPackageEvent()),
                        AddActions(new OnGitHubPageBuildEvent()),
                        AddActions(new OnGitHubPingEvent()),
                        AddActions(new OnGitHubProjectCardEvent()),
                        AddActions(new OnGitHubProjectColumnEvent()),
                        AddActions(new OnGitHubProjectEvent()),
                        AddActions(new OnGitHubPublicEvent()),
                        AddActions(new OnGitHubPullRequestEvent()),
                        AddActions(new OnGitHubPullRequestReviewCommentEvent()),
                        AddActions(new OnGitHubPullRequestReviewEvent()),
                        AddActions(new OnGitHubPushEvent()),
                        AddActions(new OnGitHubReleaseEvent()),
                        AddActions(new OnGitHubRepositoryDispatchEvent()),
                        AddActions(new OnGitHubRepositoryEvent()),
                        AddActions(new OnGitHubRepositoryImportEvent()),
                        AddActions(new OnGitHubRepositoryVulnerabilityAlertEvent()),
                        AddActions(new OnGitHubSecurityAdvisoryEvent()),
                        AddActions(new OnGitHubSponsorshipEvent()),
                        AddActions(new OnGitHubStarEvent()),
                        AddActions(new OnGitHubStatusEvent()),
                        AddActions(new OnGitHubTeamAddEvent()),
                        AddActions(new OnGitHubTeamEvent()),
                        AddActions(new OnGitHubWatchEvent()),
                        AddActions(new OnGitHubWorkflowDispatchEvent()),
                        AddActions(new OnGitHubWorkflowRunEvent()),
                        AddActions(new OnUnknownIntent()),
                        AddActions(new OnGitHubEvent()),
                        AddActions(new OnEventActivity()),
                    }
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                   {"MicrosoftAppId", context.TestName },
                    {"BotId", context.TestName },
                }).Build();

            adapter = (GitHubAdapter)new GitHubAdapter(config, new Octokit.GitHubClient(new Octokit.ProductHeaderValue("test")))
                 .UseStorage(new MemoryStorage())
                .UseBotState(new UserState(new MemoryStorage()), new ConversationState(new MemoryStorage()))
                // .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: true)))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(new ResourceExplorer()));
        }

        public async Task TestTrigger(string triggerName)
        {
            var testScript = new TestScript()
            {
                Dialog = dialog
            };

            var dataFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Data"));
            foreach (var jsonFile in Directory.EnumerateFiles(dataFolder, $"{triggerName}.*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsonFile));
                System.Diagnostics.Debug.WriteLine(Path.GetFileName(jsonFile));
                var payload = File.ReadAllText(jsonFile);
                await adapter.ProcessWebhookPayloadAsync(null, payload, async (tc, ct) =>
                {
                    //System.Diagnostics.Debug.WriteLine(name);
                    //System.Diagnostics.Debug.WriteLine((string)((dynamic)tc.Activity.Value).action);
                    //System.Diagnostics.Debug.WriteLine((string)((dynamic)tc.Activity.Value).signature);
                    //System.Diagnostics.Debug.WriteLine(String.Join(",", ((JObject)tc.Activity.Value).Properties().Select(p => p.Name).Where(p => p != "signature").OrderBy(p => p)));
                    //System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(tc.Activity, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));

                    // should return with correct trigger name
                    try
                    {
                        await testScript
                            .Send(tc.Activity)
                                .AssertReply(name)
                            .ExecuteAsync(tc.TurnState.Get<ResourceExplorer>());
                    }
                    catch (Exception err)
                    {
                        Assert.Fail($"{Path.GetFileName(jsonFile)} failed: {err.Message}");
                    }
                });
            }
        }

        //[TestMethod]
        //public async Task TestPerf()
        //{
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < 10; i++)
        //    {
        //        await TestTrigger("OnGithubCheckRunEvent");
        //    }
        //    sw.Stop();
        //    Assert.Fail(sw.Elapsed.ToString());
        //}

        [TestMethod]
        public async Task OnGithubCheckRunEvent()
        {
            await TestTrigger("OnGithubCheckRunEvent");
        }

        [TestMethod]
        public async Task OnGitHubCheckSuiteEvent()
        {
            await TestTrigger("OnGitHubCheckSuiteEvent");
        }

        [TestMethod]
        public async Task OnGitHubCodeScanningAlertEvent()
        {
            await TestTrigger("OnGitHubCodeScanningAlertEvent");
        }

        [TestMethod]
        public async Task OnGitHubCommitCommentEvent()
        {
            await TestTrigger("OnGitHubCommitCommentEvent");
        }

        [TestMethod]
        public async Task OnGitHubContentReferenceEvent()
        {
            await TestTrigger("OnGitHubContentReferenceEvent");
        }

        [TestMethod]
        public async Task OnGitHubCreateEvent()
        {
            await TestTrigger("OnGitHubCreateEvent");
        }

        [TestMethod]
        public async Task OnGitHubDeleteEvent()
        {
            await TestTrigger("OnGitHubDeleteEvent");
        }

        [TestMethod]
        public async Task OnGitHubDeployKeyEvent()
        {
            await TestTrigger("OnGitHubDeployKeyEvent");
        }

        [TestMethod]
        public async Task OnGitHubDeploymentEvent()
        {
            await TestTrigger("OnGitHubDeploymentEvent");
        }

        [TestMethod]
        public async Task OnGitHubDeploymentStatusEvent()
        {
            await TestTrigger("OnGitHubDeploymentStatusEvent");
        }

        [TestMethod]
        public async Task OnGitHubForkEvent()
        {
            await TestTrigger("OnGitHubForkEvent");
        }

        [TestMethod]
        public async Task OnGitHubGithubAppAuthorizationEvent()
        {
            await TestTrigger("OnGitHubGithubAppAuthorizationEvent");
        }

        [TestMethod]
        public async Task OnGitHubGollumEvent()
        {
            await TestTrigger("OnGitHubGollumEvent");
        }

        [TestMethod]
        public async Task OnGitHubInstallationEvent()
        {
            await TestTrigger("OnGitHubInstallationEvent");
        }

        [TestMethod]
        public async Task OnGitHubInstallationRepositoriesEvent()
        {
            await TestTrigger("OnGitHubInstallationRepositoriesEvent");
        }

        [TestMethod]
        public async Task OnGitHubIssueCommentEvent()
        {
            await TestTrigger("OnGitHubIssueCommentEvent");
        }

        [TestMethod]
        public async Task OnGitHubIssuesEvent()
        {
            await TestTrigger("OnGitHubIssuesEvent");
        }

        [TestMethod]
        public async Task OnGitHubLabelEvent()
        {
            await TestTrigger("OnGitHubLabelEvent");
        }

        [TestMethod]
        public async Task OnGitHubMarketplacePurchaseEvent()
        {
            await TestTrigger("OnGitHubMarketplacePurchaseEvent");
        }

        [TestMethod]
        public async Task OnGitHubMemberEvent()
        {
            await TestTrigger("OnGitHubMemberEvent");
        }

        [TestMethod]
        public async Task OnGitHubMembershipEvent()
        {
            await TestTrigger("OnGitHubMembershipEvent");
        }

        [TestMethod]
        public async Task OnGitHubMetaEvent()
        {
            await TestTrigger("OnGitHubMetaEvent");
        }

        [TestMethod]
        public async Task OnGitHubMilestoneEvent()
        {
            await TestTrigger("OnGitHubMilestoneEvent");
        }

        [TestMethod]
        public async Task OnGitHubOrganizationEvent()
        {
            await TestTrigger("OnGitHubOrganizationEvent");
        }

        [TestMethod]
        public async Task OnGitHubOrgBlockEvent()
        {
            await TestTrigger("OnGitHubOrgBlockEvent");
        }

        [TestMethod]
        public async Task OnGitHubPackageEvent()
        {
            await TestTrigger("OnGitHubPackageEvent");
        }

        [TestMethod]
        public async Task OnGitHubPageBuildEvent()
        {
            await TestTrigger("OnGitHubPageBuildEvent");
        }

        [TestMethod]
        public async Task OnGitHubPingEvent()
        {
            await TestTrigger("OnGitHubPingEvent");
        }

        [TestMethod]
        public async Task OnGitHubProjectCardEvent()
        {
            await TestTrigger("OnGitHubProjectCardEvent");
        }

        [TestMethod]
        public async Task OnGitHubProjectColumnEvent()
        {
            await TestTrigger("OnGitHubProjectColumnEvent");
        }

        [TestMethod]
        public async Task OnGitHubProjectEvent()
        {
            await TestTrigger("OnGitHubProjectEvent");
        }

        [TestMethod]
        public async Task OnGitHubPublicEvent()
        {
            await TestTrigger("OnGitHubPublicEvent");
        }

        [TestMethod]
        public async Task OnGitHubPullRequestEvent()
        {
            await TestTrigger("OnGitHubPullRequestEvent");
        }

        [TestMethod]
        public async Task OnGitHubPullRequestReviewCommentEvent()
        {
            await TestTrigger("OnGitHubPullRequestReviewCommentEvent");
        }

        [TestMethod]
        public async Task OnGitHubPullRequestReviewEvent()
        {
            await TestTrigger("OnGitHubPullRequestReviewEvent");
        }

        [TestMethod]
        public async Task OnGitHubPushEvent()
        {
            await TestTrigger("OnGitHubPushEvent");
        }

        [TestMethod]
        public async Task OnGitHubReleaseEvent()
        {
            await TestTrigger("OnGitHubReleaseEvent");
        }

        [TestMethod]
        public async Task OnGitHubRepositoryDispatchEvent()
        {
            await TestTrigger("OnGitHubRepositoryDispatchEvent");
        }

        [TestMethod]
        public async Task OnGitHubRepositoryEvent()
        {
            await TestTrigger("OnGitHubRepositoryEvent");
        }

        [TestMethod]
        public async Task OnGitHubRepositoryImportEvent()
        {
            await TestTrigger("OnGitHubRepositoryImportEvent");
        }

        [TestMethod]
        public async Task OnGitHubRepositoryVulnerabilityAlertEvent()
        {
            await TestTrigger("OnGitHubRepositoryVulnerabilityAlertEvent");
        }

        [TestMethod]
        public async Task OnGitHubSecurityAdvisoryEvent()
        {
            await TestTrigger("OnGitHubSecurityAdvisoryEvent");
        }

        [TestMethod]
        public async Task OnGitHubSponsorshipEvent()
        {
            await TestTrigger("OnGitHubSponsorshipEvent");
        }

        [TestMethod]
        public async Task OnGitHubStarEvent()
        {
            await TestTrigger("OnGitHubStarEvent");
        }

        [TestMethod]
        public async Task OnGitHubStatusEvent()
        {
            await TestTrigger("OnGitHubStatusEvent");
        }

        [TestMethod]
        public async Task OnGitHubTeamAddEvent()
        {
            await TestTrigger("OnGitHubTeamAddEvent");
        }

        [TestMethod]
        public async Task OnGitHubTeamEvent()
        {
            await TestTrigger("OnGitHubTeamEvent");
        }

        [TestMethod]
        public async Task OnGitHubWatchEvent()
        {
            await TestTrigger("OnGitHubWatchEvent");
        }

        [TestMethod]
        public async Task OnGitHubWorkflowDispatchEvent()
        {
            await TestTrigger("OnGitHubWorkflowDispatchEvent");
        }

        [TestMethod]
        public async Task OnGitHubWorkflowRunEvent()
        {
            await TestTrigger("OnGitHubWorkflowRunEvent");
        }
    }
}
