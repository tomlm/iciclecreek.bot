# Github extensions for Composer

## Installation
Add nuget reference.
```Iciclecreek.Bot.Builder.Dialogs.Adaptive.Github```

Add registration to startup.cs
```cs
    ComponentRegistration.Add(new GithubComponentRegistration());
```

Add GithubAdapter to startup.cs
```cs
    services.AddSingleton<GitHubAdapter>((s) =>
    {
        // create github adapter for processing webhooks
        return (GitHubAdapter)new GitHubAdapter(s.GetService<IConfiguration>())
            .UseStorage(s.GetService<IStorage>())
            .UseBotState(s.GetService<UserState>(), s.GetService<ConversationState>())
            .Use(new RegisterClassMiddleware<QueueStorage>(s.GetService<QueueStorage>()));
    });
```

Add github event callback function 
```cs
    public class GitHubTrigger
    {
        private readonly GitHubAdapter _adapter;
        private readonly IBot _bot;

        public GitHubTrigger(GitHubAdapter adapter, IBot bot)
        {
            this._adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        [FunctionName("github")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"GitHubWebHook endpoint triggered.");
            var body = await req.ReadAsStringAsync();
            var signature = req.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var response = await _adapter.ProcessWebhookPayloadAsync(signature, body, _bot.OnTurnAsync, default(CancellationToken)).ConfigureAwait(false);
            return new OkResult();
        }
```


# Triggers
This library adds event triggers for Github events.  This allows you to write a dialog for common github events:
* OnGithubCheckRunEvent 
* OnGitHubCheckSuiteEvent 
* OnGitHubCodeScanningAlertEvent 
* OnGitHubCommitCommentEvent 
* OnGitHubContentReferenceEvent 
* OnGitHubCreateEvent 
* OnGitHubDeleteEvent 
* OnGitHubDeployKeyEvent 
* OnGitHubDeploymentEvent 
* OnGitHubDeploymentStatusEvent 
* OnGitHubEvent 
* OnGitHubForkEvent 
* OnGitHubGithubAppAuthorizationEvent 
* OnGitHubGollumEvent 
* OnGitHubInstallationEvent 
* OnGitHubInstallationRepositoriesEvent 
* OnGitHubIssueCommentEvent 
* OnGitHubIssuesEvent 
* OnGitHubLabelEvent 
* OnGitHubMarketplacePurchaseEvent 
* OnGitHubMemberEvent 
* OnGitHubMembershipEvent 
* OnGitHubMetaEvent 
* OnGitHubMilestoneEvent 
* OnGitHubOrganizationEvent 
* OnGitHubOrgBlockEvent 
* OnGitHubPackageEvent 
* OnGitHubPageBuildEvent 
* OnGitHubPingEvent 
* OnGitHubProjectCardEvent 
* OnGitHubProjectColumnEvent 
* OnGitHubProjectEvent 
* OnGitHubPublicEvent 
* OnGitHubPullRequestEvent 
* OnGitHubPullRequestReviewCommentEvent 
* OnGitHubPullRequestReviewEvent 
* OnGitHubPushEvent 
* OnGitHubReleaseEvent 
* OnGitHubRepositoryDispatchEvent 
* OnGitHubRepositoryEvent 
* OnGitHubRepositoryImportEvent 
* OnGitHubRepositoryVulnerabilityAlertEvent 
* OnGitHubSecurityAdvisoryEvent 
* OnGitHubSponsorshipEvent 
* OnGitHubStarEvent 
* OnGitHubStatusEvent 
* OnGitHubTeamAddEvent 
* OnGitHubTeamEvent 
* OnGitHubWatchEvent 
* OnGitHubWorkflowDispatchEvent 
* OnGitHubWorkflowRunEvent 

# Actions
This library also adds 450 actions for using Github APIs. 

# Sample
```json
    {
      "$kind": "GitHub.OnIssueCommentEvent",
      "condition": "turn.activity.value.user.login != 'RepoBot'",
      "actions": [
        {
          "$kind": "GitHub.Issue.Comment.Create",
          "repositoryId": "=turn.activity.value.repository.id",
          "number": "=turn.activity.value.issue.id",
          "newComment": "You said:\n${turn.activity.value.comment.body}"
        }
      ]
    }
```
