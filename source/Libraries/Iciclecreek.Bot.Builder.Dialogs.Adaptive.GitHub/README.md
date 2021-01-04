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
* GitHub.OnGithubCheckRunEvent 
* GitHub.OnGitHubCheckSuiteEvent 
* GitHub.OnGitHubCodeScanningAlertEvent 
* GitHub.OnGitHubCommitCommentEvent 
* GitHub.OnGitHubContentReferenceEvent 
* GitHub.OnGitHubCreateEvent 
* GitHub.OnGitHubDeleteEvent 
* GitHub.OnGitHubDeployKeyEvent 
* GitHub.OnGitHubDeploymentEvent 
* GitHub.OnGitHubDeploymentStatusEvent 
* GitHub.OnGitHubEvent 
* GitHub.OnGitHubForkEvent 
* GitHub.OnGitHubGithubAppAuthorizationEvent 
* GitHub.OnGitHubGollumEvent 
* GitHub.OnGitHubInstallationEvent 
* GitHub.OnGitHubInstallationRepositoriesEvent 
* GitHub.OnGitHubIssueCommentEvent 
* GitHub.OnGitHubIssuesEvent 
* GitHub.OnGitHubLabelEvent 
* GitHub.OnGitHubMarketplacePurchaseEvent 
* GitHub.OnGitHubMemberEvent 
* GitHub.OnGitHubMembershipEvent 
* GitHub.OnGitHubMetaEvent 
* GitHub.OnGitHubMilestoneEvent 
* GitHub.OnGitHubOrganizationEvent 
* GitHub.OnGitHubOrgBlockEvent 
* GitHub.OnGitHubPackageEvent 
* GitHub.OnGitHubPageBuildEvent 
* GitHub.OnGitHubPingEvent 
* GitHub.OnGitHubProjectCardEvent 
* GitHub.OnGitHubProjectColumnEvent 
* GitHub.OnGitHubProjectEvent 
* GitHub.OnGitHubPublicEvent 
* GitHub.OnGitHubPullRequestEvent 
* GitHub.OnGitHubPullRequestReviewCommentEvent 
* GitHub.OnGitHubPullRequestReviewEvent 
* GitHub.OnGitHubPushEvent 
* GitHub.OnGitHubReleaseEvent 
* GitHub.OnGitHubRepositoryDispatchEvent 
* GitHub.OnGitHubRepositoryEvent 
* GitHub.OnGitHubRepositoryImportEvent 
* GitHub.OnGitHubRepositoryVulnerabilityAlertEvent 
* GitHub.OnGitHubSecurityAdvisoryEvent 
* GitHub.OnGitHubSponsorshipEvent 
* GitHub.OnGitHubStarEvent 
* GitHub.OnGitHubStatusEvent 
* GitHub.OnGitHubTeamAddEvent 
* GitHub.OnGitHubTeamEvent 
* GitHub.OnGitHubWatchEvent 
* GitHub.OnGitHubWorkflowDispatchEvent 
* GitHub.OnGitHubWorkflowRunEvent 

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
