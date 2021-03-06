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


# Triggers
This library adds event triggers for Github events.  This allows you to write a dialog for common github events:
* GitHub.Activity.Events.GetAll 
* GitHub.Activity.Events.GetAllForAnOrganization 
* GitHub.Activity.Events.GetAllForOrganization 
* GitHub.Activity.Events.GetAllForRepository 
* GitHub.Activity.Events.GetAllForRepositoryNetwork 
* GitHub.Activity.Events.GetAllIssuesForRepository 
* GitHub.Activity.Events.GetAllUserPerformed 
* GitHub.Activity.Events.GetAllUserPerformedPublic 
* GitHub.Activity.Events.GetAllUserReceived 
* GitHub.Activity.Events.GetAllUserReceivedPublic 
* GitHub.Activity.Feeds.GetFeeds 
* GitHub.Activity.Notifications.DeleteThreadSubscription 
* GitHub.Activity.Notifications.Get 
* GitHub.Activity.Notifications.GetAllForCurrent 
* GitHub.Activity.Notifications.GetAllForRepository 
* GitHub.Activity.Notifications.GetThreadSubscription 
* GitHub.Activity.Notifications.MarkAsRead 
* GitHub.Activity.Notifications.MarkAsReadForRepository 
* GitHub.Activity.Notifications.SetThreadSubscription 
* GitHub.Activity.Starring.CheckStarred 
* GitHub.Activity.Starring.GetAllForCurrent 
* GitHub.Activity.Starring.GetAllForCurrentWithTimestamps 
* GitHub.Activity.Starring.GetAllForUser 
* GitHub.Activity.Starring.GetAllForUserWithTimestamps 
* GitHub.Activity.Starring.GetAllStargazers 
* GitHub.Activity.Starring.GetAllStargazersWithTimestamps 
* GitHub.Activity.Starring.RemoveStarFromRepo 
* GitHub.Activity.Starring.StarRepo 
* GitHub.Activity.Watching.CheckWatched 
* GitHub.Activity.Watching.GetAllForCurrent 
* GitHub.Activity.Watching.GetAllForUser 
* GitHub.Activity.Watching.GetAllWatchers 
* GitHub.Activity.Watching.UnwatchRepo 
* GitHub.Activity.Watching.WatchRepo 
* GitHub.Check.Run.Create 
* GitHub.Check.Run.Get 
* GitHub.Check.Run.GetAllAnnotations 
* GitHub.Check.Run.GetAllForCheckSuite 
* GitHub.Check.Run.GetAllForReference 
* GitHub.Check.Run.Update 
* GitHub.Check.Suite.Create 
* GitHub.Check.Suite.Get 
* GitHub.Check.Suite.GetAllForReference 
* GitHub.Check.Suite.Rerequest 
* GitHub.Check.Suite.UpdatePreferences 
* GitHub.Enterprise.AdminStats.GetStatisticsAll 
* GitHub.Enterprise.AdminStats.GetStatisticsComments 
* GitHub.Enterprise.AdminStats.GetStatisticsGists 
* GitHub.Enterprise.AdminStats.GetStatisticsHooks 
* GitHub.Enterprise.AdminStats.GetStatisticsIssues 
* GitHub.Enterprise.AdminStats.GetStatisticsMilestones 
* GitHub.Enterprise.AdminStats.GetStatisticsOrgs 
* GitHub.Enterprise.AdminStats.GetStatisticsPages 
* GitHub.Enterprise.AdminStats.GetStatisticsPulls 
* GitHub.Enterprise.AdminStats.GetStatisticsRepos 
* GitHub.Enterprise.AdminStats.GetStatisticsUsers 
* GitHub.Enterprise.Ldap.QueueSyncTeamMapping 
* GitHub.Enterprise.Ldap.QueueSyncUserMapping 
* GitHub.Enterprise.Ldap.UpdateTeamMapping 
* GitHub.Enterprise.Ldap.UpdateUserMapping 
* GitHub.Enterprise.License.Get 
* GitHub.Enterprise.ManagementConsole.EditMaintenanceMode 
* GitHub.Enterprise.ManagementConsole.GetMaintenanceMode 
* GitHub.Enterprise.Organization.Create 
* GitHub.Enterprise.PreReceiveEnvironment.Create 
* GitHub.Enterprise.PreReceiveEnvironment.Delete 
* GitHub.Enterprise.PreReceiveEnvironment.DownloadStatus 
* GitHub.Enterprise.PreReceiveEnvironment.Edit 
* GitHub.Enterprise.PreReceiveEnvironment.Get 
* GitHub.Enterprise.PreReceiveEnvironment.GetAll 
* GitHub.Enterprise.PreReceiveEnvironment.TriggerDownload 
* GitHub.Enterprise.SearchIndexing.Queue 
* GitHub.Enterprise.SearchIndexing.QueueAll 
* GitHub.Enterprise.SearchIndexing.QueueAllCode 
* GitHub.Enterprise.SearchIndexing.QueueAllIssues 
* GitHub.Gist.Comment.Create 
* GitHub.Gist.Comment.Delete 
* GitHub.Gist.Comment.Get 
* GitHub.Gist.Comment.GetAllForGist 
* GitHub.Gist.Comment.Update 
* GitHub.Gist.Create 
* GitHub.Gist.Delete 
* GitHub.Gist.Edit 
* GitHub.Gist.Fork 
* GitHub.Gist.Get 
* GitHub.Gist.GetAll 
* GitHub.Gist.GetAllCommits 
* GitHub.Gist.GetAllForUser 
* GitHub.Gist.GetAllForks 
* GitHub.Gist.GetAllPublic 
* GitHub.Gist.GetAllStarred 
* GitHub.Gist.IsStarred 
* GitHub.Gist.Star 
* GitHub.Gist.Unstar 
* GitHub.Git.Blob.Create 
* GitHub.Git.Blob.Get 
* GitHub.Git.Commit.Create 
* GitHub.Git.Commit.Get 
* GitHub.Git.Reference.Create 
* GitHub.Git.Reference.Delete 
* GitHub.Git.Reference.Get 
* GitHub.Git.Reference.GetAll 
* GitHub.Git.Reference.GetAllForSubNamespace 
* GitHub.Git.Reference.Update 
* GitHub.Git.Tag.Create 
* GitHub.Git.Tag.Get 
* GitHub.Git.Tree.Create 
* GitHub.Git.Tree.Get 
* GitHub.Git.Tree.GetRecursive 
* GitHub.GitHubApps.CreateInstallationToken 
* GitHub.GitHubApps.Get 
* GitHub.GitHubApps.GetAllInstallationsForCurrent 
* GitHub.GitHubApps.GetAllInstallationsForCurrentUser 
* GitHub.GitHubApps.GetCurrent 
* GitHub.GitHubApps.GetInstallation 
* GitHub.GitHubApps.GetInstallationForCurrent 
* GitHub.GitHubApps.GetOrganizationInstallationForCurrent 
* GitHub.GitHubApps.GetRepositoryInstallationForCurrent 
* GitHub.GitHubApps.GetUserInstallationForCurrent 
* GitHub.GitHubApps.Installation.GetAllRepositoriesForCurrent 
* GitHub.GitHubApps.Installation.GetAllRepositoriesForCurrentUser 
* GitHub.Issue.Assignee.AddAssignees 
* GitHub.Issue.Assignee.CheckAssignee 
* GitHub.Issue.Assignee.GetAllForRepository 
* GitHub.Issue.Assignee.RemoveAssignees 
* GitHub.Issue.Comment.Create 
* GitHub.Issue.Comment.Delete 
* GitHub.Issue.Comment.Get 
* GitHub.Issue.Comment.GetAllForIssue 
* GitHub.Issue.Comment.GetAllForRepository 
* GitHub.Issue.Comment.Update 
* GitHub.Issue.Create 
* GitHub.Issue.Events.Get 
* GitHub.Issue.Events.GetAllForIssue 
* GitHub.Issue.Events.GetAllForRepository 
* GitHub.Issue.Get 
* GitHub.Issue.GetAllForCurrent 
* GitHub.Issue.GetAllForOrganization 
* GitHub.Issue.GetAllForOwnedAndMemberRepositories 
* GitHub.Issue.GetAllForRepository 
* GitHub.Issue.Labels.AddToIssue 
* GitHub.Issue.Labels.Create 
* GitHub.Issue.Labels.Delete 
* GitHub.Issue.Labels.Get 
* GitHub.Issue.Labels.GetAllForIssue 
* GitHub.Issue.Labels.GetAllForMilestone 
* GitHub.Issue.Labels.GetAllForRepository 
* GitHub.Issue.Labels.RemoveAllFromIssue 
* GitHub.Issue.Labels.RemoveFromIssue 
* GitHub.Issue.Labels.ReplaceAllForIssue 
* GitHub.Issue.Labels.Update 
* GitHub.Issue.Lock 
* GitHub.Issue.Milestone.Create 
* GitHub.Issue.Milestone.Delete 
* GitHub.Issue.Milestone.Get 
* GitHub.Issue.Milestone.GetAllForRepository 
* GitHub.Issue.Milestone.Update 
* GitHub.Issue.Timeline.GetAllForIssue 
* GitHub.Issue.Unlock 
* GitHub.Issue.Update 
* GitHub.Migration.Migrations.DeleteArchive 
* GitHub.Migration.Migrations.Get 
* GitHub.Migration.Migrations.GetAll 
* GitHub.Migration.Migrations.GetArchive 
* GitHub.Migration.Migrations.Start 
* GitHub.Migration.Migrations.UnlockRepository 
* GitHub.Miscellaneous.GetAllEmojis 
* GitHub.Miscellaneous.GetAllGitIgnoreTemplates 
* GitHub.Miscellaneous.GetAllLicenses 
* GitHub.Miscellaneous.GetGitIgnoreTemplate 
* GitHub.Miscellaneous.GetLicense 
* GitHub.Miscellaneous.GetMetadata 
* GitHub.Miscellaneous.GetRateLimits 
* GitHub.Miscellaneous.RenderArbitraryMarkdown 
* GitHub.Miscellaneous.RenderRawMarkdown 
* GitHub.Oauth.CreateAccessToken 
* GitHub.Oauth.GetGitHubLoginUrl 
* GitHub.OnCheckRunEvent 
* GitHub.OnCheckSuiteEvent 
* GitHub.OnCodeScanningAlertEvent 
* GitHub.OnCommitCommentEvent 
* GitHub.OnContentReferenceEvent 
* GitHub.OnCreateEvent 
* GitHub.OnDeleteEvent 
* GitHub.OnDeployKeyEvent 
* GitHub.OnDeploymentEvent 
* GitHub.OnDeploymentStatusEvent 
* GitHub.OnForkEvent 
* GitHub.OnGithubAppAuthorizationEvent 
* GitHub.OnGollumEvent 
* GitHub.OnInstallationEvent 
* GitHub.OnInstallationRepositoriesEvent 
* GitHub.OnIssueCommentEvent 
* GitHub.OnIssuesEvent 
* GitHub.OnLabelEvent 
* GitHub.OnMarketplacePurchaseEvent 
* GitHub.OnMemberEvent 
* GitHub.OnMembershipEvent 
* GitHub.OnMetaEvent 
* GitHub.OnMilestoneEvent 
* GitHub.OnOrgBlockEvent 
* GitHub.OnOrganizationEvent 
* GitHub.OnPackageEvent 
* GitHub.OnPageBuildEvent 
* GitHub.OnPingEvent 
* GitHub.OnProjectCardEvent 
* GitHub.OnProjectColumnEvent 
* GitHub.OnProjectEvent 
* GitHub.OnPublicEvent 
* GitHub.OnPullRequestEvent 
* GitHub.OnPullRequestReviewCommentEvent 
* GitHub.OnPullRequestReviewEvent 
* GitHub.OnPushEvent 
* GitHub.OnReleaseEvent 
* GitHub.OnRepositoryDispatchEvent 
* GitHub.OnRepositoryEvent 
* GitHub.OnRepositoryImportEvent 
* GitHub.OnRepositoryVulnerabilityAlertEvent 
* GitHub.OnSecurityAdvisoryEvent 
* GitHub.OnSponsorshipEvent 
* GitHub.OnStarEvent 
* GitHub.OnStatusEvent 
* GitHub.OnTeamAddEvent 
* GitHub.OnTeamEvent 
* GitHub.OnWatchEvent 
* GitHub.OnWorkflowDispatchEvent 
* GitHub.OnWorkflowRunEvent 
* GitHub.Organization.Get 
* GitHub.Organization.GetAll 
* GitHub.Organization.GetAllForCurrent 
* GitHub.Organization.GetAllForUser 
* GitHub.Organization.Hook.Create 
* GitHub.Organization.Hook.Delete 
* GitHub.Organization.Hook.Edit 
* GitHub.Organization.Hook.Get 
* GitHub.Organization.Hook.GetAll 
* GitHub.Organization.Hook.Ping 
* GitHub.Organization.Member.AddOrUpdateOrganizationMembership 
* GitHub.Organization.Member.CheckMember 
* GitHub.Organization.Member.CheckMemberPublic 
* GitHub.Organization.Member.Conceal 
* GitHub.Organization.Member.Delete 
* GitHub.Organization.Member.GetAll 
* GitHub.Organization.Member.GetAllPendingInvitations 
* GitHub.Organization.Member.GetAllPublic 
* GitHub.Organization.Member.GetOrganizationMembership 
* GitHub.Organization.Member.Publicize 
* GitHub.Organization.Member.RemoveOrganizationMembership 
* GitHub.Organization.OutsideCollaborator.ConvertFromMember 
* GitHub.Organization.OutsideCollaborator.Delete 
* GitHub.Organization.OutsideCollaborator.GetAll 
* GitHub.Organization.Team.AddOrEditMembership 
* GitHub.Organization.Team.AddRepository 
* GitHub.Organization.Team.Create 
* GitHub.Organization.Team.Delete 
* GitHub.Organization.Team.Get 
* GitHub.Organization.Team.GetAll 
* GitHub.Organization.Team.GetAllChildTeams 
* GitHub.Organization.Team.GetAllForCurrent 
* GitHub.Organization.Team.GetAllMembers 
* GitHub.Organization.Team.GetAllPendingInvitations 
* GitHub.Organization.Team.GetAllRepositories 
* GitHub.Organization.Team.GetMembershipDetails 
* GitHub.Organization.Team.IsRepositoryManagedByTeam 
* GitHub.Organization.Team.RemoveMembership 
* GitHub.Organization.Team.RemoveRepository 
* GitHub.Organization.Team.Update 
* GitHub.Organization.Update 
* GitHub.PullRequest.Commits 
* GitHub.PullRequest.Create 
* GitHub.PullRequest.Files 
* GitHub.PullRequest.Get 
* GitHub.PullRequest.GetAllForRepository 
* GitHub.PullRequest.Merge 
* GitHub.PullRequest.Merged 
* GitHub.PullRequest.Review.Create 
* GitHub.PullRequest.Review.Delete 
* GitHub.PullRequest.Review.Dismiss 
* GitHub.PullRequest.Review.Get 
* GitHub.PullRequest.Review.GetAll 
* GitHub.PullRequest.Review.GetAllComments 
* GitHub.PullRequest.Review.Submit 
* GitHub.PullRequest.ReviewComment.Create 
* GitHub.PullRequest.ReviewComment.CreateReply 
* GitHub.PullRequest.ReviewComment.Delete 
* GitHub.PullRequest.ReviewComment.Edit 
* GitHub.PullRequest.ReviewComment.GetAll 
* GitHub.PullRequest.ReviewComment.GetAllForRepository 
* GitHub.PullRequest.ReviewComment.GetComment 
* GitHub.PullRequest.ReviewRequest.Create 
* GitHub.PullRequest.ReviewRequest.Delete 
* GitHub.PullRequest.ReviewRequest.Get 
* GitHub.PullRequest.Update 
* GitHub.Reaction.CommitComment.Create 
* GitHub.Reaction.CommitComment.GetAll 
* GitHub.Reaction.Delete 
* GitHub.Reaction.Issue.Create 
* GitHub.Reaction.Issue.GetAll 
* GitHub.Reaction.IssueComment.Create 
* GitHub.Reaction.IssueComment.GetAll 
* GitHub.Reaction.PullRequestReviewComment.Create 
* GitHub.Reaction.PullRequestReviewComment.GetAll 
* GitHub.Repository.Branch.AddAdminEnforcement 
* GitHub.Repository.Branch.AddProtectedBranchTeamRestrictions 
* GitHub.Repository.Branch.AddProtectedBranchUserRestrictions 
* GitHub.Repository.Branch.AddRequiredStatusChecksContexts 
* GitHub.Repository.Branch.DeleteBranchProtection 
* GitHub.Repository.Branch.DeleteProtectedBranchRestrictions 
* GitHub.Repository.Branch.DeleteProtectedBranchTeamRestrictions 
* GitHub.Repository.Branch.DeleteProtectedBranchUserRestrictions 
* GitHub.Repository.Branch.DeleteRequiredStatusChecks 
* GitHub.Repository.Branch.DeleteRequiredStatusChecksContexts 
* GitHub.Repository.Branch.Get 
* GitHub.Repository.Branch.GetAdminEnforcement 
* GitHub.Repository.Branch.GetAll 
* GitHub.Repository.Branch.GetAllProtectedBranchTeamRestrictions 
* GitHub.Repository.Branch.GetAllProtectedBranchUserRestrictions 
* GitHub.Repository.Branch.GetAllRequiredStatusChecksContexts 
* GitHub.Repository.Branch.GetBranchProtection 
* GitHub.Repository.Branch.GetProtectedBranchRestrictions 
* GitHub.Repository.Branch.GetRequiredStatusChecks 
* GitHub.Repository.Branch.GetReviewEnforcement 
* GitHub.Repository.Branch.RemoveAdminEnforcement 
* GitHub.Repository.Branch.RemoveReviewEnforcement 
* GitHub.Repository.Branch.UpdateBranchProtection 
* GitHub.Repository.Branch.UpdateProtectedBranchTeamRestrictions 
* GitHub.Repository.Branch.UpdateProtectedBranchUserRestrictions 
* GitHub.Repository.Branch.UpdateRequiredStatusChecks 
* GitHub.Repository.Branch.UpdateRequiredStatusChecksContexts 
* GitHub.Repository.Branch.UpdateReviewEnforcement 
* GitHub.Repository.Collaborator.Add 
* GitHub.Repository.Collaborator.Delete 
* GitHub.Repository.Collaborator.GetAll 
* GitHub.Repository.Collaborator.Invite 
* GitHub.Repository.Collaborator.IsCollaborator 
* GitHub.Repository.Collaborator.ReviewPermission 
* GitHub.Repository.Comment.Create 
* GitHub.Repository.Comment.Delete 
* GitHub.Repository.Comment.Get 
* GitHub.Repository.Comment.GetAllForCommit 
* GitHub.Repository.Comment.GetAllForRepository 
* GitHub.Repository.Comment.Update 
* GitHub.Repository.Commit.Compare 
* GitHub.Repository.Commit.Get 
* GitHub.Repository.Commit.GetAll 
* GitHub.Repository.Commit.GetSha1 
* GitHub.Repository.Content.CreateFile 
* GitHub.Repository.Content.DeleteFile 
* GitHub.Repository.Content.GetAllContents 
* GitHub.Repository.Content.GetAllContentsByRef 
* GitHub.Repository.Content.GetArchive 
* GitHub.Repository.Content.GetRawContent 
* GitHub.Repository.Content.GetRawContentByRef 
* GitHub.Repository.Content.GetReadme 
* GitHub.Repository.Content.GetReadmeHtml 
* GitHub.Repository.Content.UpdateFile 
* GitHub.Repository.Create 
* GitHub.Repository.Delete 
* GitHub.Repository.DeployKeys.Create 
* GitHub.Repository.DeployKeys.Delete 
* GitHub.Repository.DeployKeys.Get 
* GitHub.Repository.DeployKeys.GetAll 
* GitHub.Repository.Deployment.Create 
* GitHub.Repository.Deployment.GetAll 
* GitHub.Repository.Deployment.Status.Create 
* GitHub.Repository.Deployment.Status.GetAll 
* GitHub.Repository.Edit 
* GitHub.Repository.Forks.Create 
* GitHub.Repository.Forks.GetAll 
* GitHub.Repository.Get 
* GitHub.Repository.GetAllContributors 
* GitHub.Repository.GetAllForCurrent 
* GitHub.Repository.GetAllForOrg 
* GitHub.Repository.GetAllForUser 
* GitHub.Repository.GetAllLanguages 
* GitHub.Repository.GetAllPublic 
* GitHub.Repository.GetAllTags 
* GitHub.Repository.GetAllTeams 
* GitHub.Repository.GetLicenseContents 
* GitHub.Repository.Hooks.Create 
* GitHub.Repository.Hooks.Delete 
* GitHub.Repository.Hooks.Edit 
* GitHub.Repository.Hooks.Get 
* GitHub.Repository.Hooks.GetAll 
* GitHub.Repository.Hooks.Ping 
* GitHub.Repository.Hooks.Test 
* GitHub.Repository.Invitation.Accept 
* GitHub.Repository.Invitation.Decline 
* GitHub.Repository.Invitation.Delete 
* GitHub.Repository.Invitation.Edit 
* GitHub.Repository.Invitation.GetAllForCurrent 
* GitHub.Repository.Invitation.GetAllForRepository 
* GitHub.Repository.Merging.Create 
* GitHub.Repository.Page.Get 
* GitHub.Repository.Page.GetAll 
* GitHub.Repository.Page.GetLatest 
* GitHub.Repository.Page.RequestPageBuild 
* GitHub.Repository.Project.Card.Create 
* GitHub.Repository.Project.Card.Delete 
* GitHub.Repository.Project.Card.Get 
* GitHub.Repository.Project.Card.GetAll 
* GitHub.Repository.Project.Card.Move 
* GitHub.Repository.Project.Card.Update 
* GitHub.Repository.Project.Column.Create 
* GitHub.Repository.Project.Column.Delete 
* GitHub.Repository.Project.Column.Get 
* GitHub.Repository.Project.Column.GetAll 
* GitHub.Repository.Project.Column.Move 
* GitHub.Repository.Project.Column.Update 
* GitHub.Repository.Project.CreateForOrganization 
* GitHub.Repository.Project.CreateForRepository 
* GitHub.Repository.Project.Delete 
* GitHub.Repository.Project.Get 
* GitHub.Repository.Project.GetAllForOrganization 
* GitHub.Repository.Project.GetAllForRepository 
* GitHub.Repository.Project.Update 
* GitHub.Repository.PullRequest.Commits 
* GitHub.Repository.PullRequest.Create 
* GitHub.Repository.PullRequest.Files 
* GitHub.Repository.PullRequest.Get 
* GitHub.Repository.PullRequest.GetAllForRepository 
* GitHub.Repository.PullRequest.Merge 
* GitHub.Repository.PullRequest.Merged 
* GitHub.Repository.PullRequest.Review.Create 
* GitHub.Repository.PullRequest.Review.Delete 
* GitHub.Repository.PullRequest.Review.Dismiss 
* GitHub.Repository.PullRequest.Review.Get 
* GitHub.Repository.PullRequest.Review.GetAll 
* GitHub.Repository.PullRequest.Review.GetAllComments 
* GitHub.Repository.PullRequest.Review.Submit 
* GitHub.Repository.PullRequest.ReviewComment.Create 
* GitHub.Repository.PullRequest.ReviewComment.CreateReply 
* GitHub.Repository.PullRequest.ReviewComment.Delete 
* GitHub.Repository.PullRequest.ReviewComment.Edit 
* GitHub.Repository.PullRequest.ReviewComment.GetAll 
* GitHub.Repository.PullRequest.ReviewComment.GetAllForRepository 
* GitHub.Repository.PullRequest.ReviewComment.GetComment 
* GitHub.Repository.PullRequest.ReviewRequest.Create 
* GitHub.Repository.PullRequest.ReviewRequest.Delete 
* GitHub.Repository.PullRequest.ReviewRequest.Get 
* GitHub.Repository.PullRequest.Update 
* GitHub.Repository.Release.Create 
* GitHub.Repository.Release.Delete 
* GitHub.Repository.Release.DeleteAsset 
* GitHub.Repository.Release.Edit 
* GitHub.Repository.Release.EditAsset 
* GitHub.Repository.Release.Get 
* GitHub.Repository.Release.GetAll 
* GitHub.Repository.Release.GetAllAssets 
* GitHub.Repository.Release.GetAsset 
* GitHub.Repository.Release.GetLatest 
* GitHub.Repository.Release.UploadAsset 
* GitHub.Repository.Statistics.GetCodeFrequency 
* GitHub.Repository.Statistics.GetCommitActivity 
* GitHub.Repository.Statistics.GetContributors 
* GitHub.Repository.Statistics.GetParticipation 
* GitHub.Repository.Statistics.GetPunchCard 
* GitHub.Repository.Status.Create 
* GitHub.Repository.Status.GetAll 
* GitHub.Repository.Status.GetCombined 
* GitHub.Repository.Traffic.GetAllPaths 
* GitHub.Repository.Traffic.GetAllReferrers 
* GitHub.Repository.Traffic.GetClones 
* GitHub.Repository.Traffic.GetViews 
* GitHub.Repository.Transfer 
* GitHub.Search.SearchCode 
* GitHub.Search.SearchIssues 
* GitHub.Search.SearchLabels 
* GitHub.Search.SearchRepo 
* GitHub.Search.SearchUsers 
* GitHub.User.Administration.Create 
* GitHub.User.Administration.CreateImpersonationToken 
* GitHub.User.Administration.Delete 
* GitHub.User.Administration.DeleteImpersonationToken 
* GitHub.User.Administration.DeletePublicKey 
* GitHub.User.Administration.Demote 
* GitHub.User.Administration.ListAllPublicKeys 
* GitHub.User.Administration.Promote 
* GitHub.User.Administration.Rename 
* GitHub.User.Administration.Suspend 
* GitHub.User.Administration.Unsuspend 
* GitHub.User.Current 
* GitHub.User.Email.Add 
* GitHub.User.Email.Delete 
* GitHub.User.Email.GetAll 
* GitHub.User.Followers.Follow 
* GitHub.User.Followers.GetAll 
* GitHub.User.Followers.GetAllFollowing 
* GitHub.User.Followers.GetAllFollowingForCurrent 
* GitHub.User.Followers.GetAllForCurrent 
* GitHub.User.Followers.IsFollowing 
* GitHub.User.Followers.IsFollowingForCurrent 
* GitHub.User.Followers.Unfollow 
* GitHub.User.Get 
* GitHub.User.GitSshKey.Create 
* GitHub.User.GitSshKey.Delete 
* GitHub.User.GitSshKey.Get 
* GitHub.User.GitSshKey.GetAll 
* GitHub.User.GitSshKey.GetAllForCurrent 
* GitHub.User.GpgKey.Create 
* GitHub.User.GpgKey.Delete 
* GitHub.User.GpgKey.Get 
* GitHub.User.GpgKey.GetAllForCurrent 
* GitHub.User.Update 


