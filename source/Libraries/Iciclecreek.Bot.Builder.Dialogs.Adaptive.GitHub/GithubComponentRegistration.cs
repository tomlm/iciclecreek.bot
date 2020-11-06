using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub.Triggers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub
{
    /// <summary>
    /// Class which contains registration of components for github.
    /// </summary>
    public class GithubComponentRegsitration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Gets declarative type registrations.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <returns>enumeration of DeclarativeTypes.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<OnGitHubCheckRunEvent>(OnGitHubCheckRunEvent.Kind);
            yield return new DeclarativeType<OnGitHubCheckSuiteEvent>(OnGitHubCheckSuiteEvent.Kind);
            yield return new DeclarativeType<OnGitHubCodeScanningAlertEvent>(OnGitHubCodeScanningAlertEvent.Kind);
            yield return new DeclarativeType<OnGitHubCommitCommentEvent>(OnGitHubCommitCommentEvent.Kind);
            yield return new DeclarativeType<OnGitHubContentReferenceEvent>(OnGitHubContentReferenceEvent.Kind);
            yield return new DeclarativeType<OnGitHubCreateEvent>(OnGitHubCreateEvent.Kind);
            yield return new DeclarativeType<OnGitHubDeleteEvent>(OnGitHubDeleteEvent.Kind);
            yield return new DeclarativeType<OnGitHubDeployKeyEvent>(OnGitHubDeployKeyEvent.Kind);
            yield return new DeclarativeType<OnGitHubDeploymentEvent>(OnGitHubDeploymentEvent.Kind);
            yield return new DeclarativeType<OnGitHubDeploymentStatusEvent>(OnGitHubDeploymentStatusEvent.Kind);
            yield return new DeclarativeType<OnGitHubEvent>(OnGitHubEvent.Kind);
            yield return new DeclarativeType<OnGitHubForkEvent>(OnGitHubForkEvent.Kind);
            yield return new DeclarativeType<OnGitHubGithubAppAuthorizationEvent>(OnGitHubGithubAppAuthorizationEvent.Kind);
            yield return new DeclarativeType<OnGitHubGollumEvent>(OnGitHubGollumEvent.Kind);
            yield return new DeclarativeType<OnGitHubInstallationEvent>(OnGitHubInstallationEvent.Kind);
            yield return new DeclarativeType<OnGitHubInstallationRepositoriesEvent>(OnGitHubInstallationRepositoriesEvent.Kind);
            yield return new DeclarativeType<OnGitHubIssueCommentEvent>(OnGitHubIssueCommentEvent.Kind);
            yield return new DeclarativeType<OnGitHubIssuesEvent>(OnGitHubIssuesEvent.Kind);
            yield return new DeclarativeType<OnGitHubLabelEvent>(OnGitHubLabelEvent.Kind);
            yield return new DeclarativeType<OnGitHubMarketplacePurchaseEvent>(OnGitHubMarketplacePurchaseEvent.Kind);
            yield return new DeclarativeType<OnGitHubMemberEvent>(OnGitHubMemberEvent.Kind);
            yield return new DeclarativeType<OnGitHubMembershipEvent>(OnGitHubMembershipEvent.Kind);
            yield return new DeclarativeType<OnGitHubMetaEvent>(OnGitHubMetaEvent.Kind);
            yield return new DeclarativeType<OnGitHubMilestoneEvent>(OnGitHubMilestoneEvent.Kind);
            yield return new DeclarativeType<OnGitHubOrganizationEvent>(OnGitHubOrganizationEvent.Kind);
            yield return new DeclarativeType<OnGitHubOrgBlockEvent>(OnGitHubOrgBlockEvent.Kind);
            yield return new DeclarativeType<OnGitHubPackageEvent>(OnGitHubPackageEvent.Kind);
            yield return new DeclarativeType<OnGitHubPageBuildEvent>(OnGitHubPageBuildEvent.Kind);
            yield return new DeclarativeType<OnGitHubPingEvent>(OnGitHubPingEvent.Kind);
            yield return new DeclarativeType<OnGitHubProjectCardEvent>(OnGitHubProjectCardEvent.Kind);
            yield return new DeclarativeType<OnGitHubProjectColumnEvent>(OnGitHubProjectColumnEvent.Kind);
            yield return new DeclarativeType<OnGitHubProjectEvent>(OnGitHubProjectEvent.Kind);
            yield return new DeclarativeType<OnGitHubPublicEvent>(OnGitHubPublicEvent.Kind);
            yield return new DeclarativeType<OnGitHubPullRequestEvent>(OnGitHubPullRequestEvent.Kind);
            yield return new DeclarativeType<OnGitHubPullRequestReviewCommentEvent>(OnGitHubPullRequestReviewCommentEvent.Kind);
            yield return new DeclarativeType<OnGitHubPullRequestReviewEvent>(OnGitHubPullRequestReviewEvent.Kind);
            yield return new DeclarativeType<OnGitHubPushEvent>(OnGitHubPushEvent.Kind);
            yield return new DeclarativeType<OnGitHubReleaseEvent>(OnGitHubReleaseEvent.Kind);
            yield return new DeclarativeType<OnGitHubRepositoryDispatchEvent>(OnGitHubRepositoryDispatchEvent.Kind);
            yield return new DeclarativeType<OnGitHubRepositoryEvent>(OnGitHubRepositoryEvent.Kind);
            yield return new DeclarativeType<OnGitHubRepositoryImportEvent>(OnGitHubRepositoryImportEvent.Kind);
            yield return new DeclarativeType<OnGitHubRepositoryVulnerabilityAlertEvent>(OnGitHubRepositoryVulnerabilityAlertEvent.Kind);
            yield return new DeclarativeType<OnGitHubSecurityAdvisoryEvent>(OnGitHubSecurityAdvisoryEvent.Kind);
            yield return new DeclarativeType<OnGitHubSponsorshipEvent>(OnGitHubSponsorshipEvent.Kind);
            yield return new DeclarativeType<OnGitHubStarEvent>(OnGitHubStarEvent.Kind);
            yield return new DeclarativeType<OnGitHubStatusEvent>(OnGitHubStatusEvent.Kind);
            yield return new DeclarativeType<OnGitHubTeamAddEvent>(OnGitHubTeamAddEvent.Kind);
            yield return new DeclarativeType<OnGitHubTeamEvent>(OnGitHubTeamEvent.Kind);
            yield return new DeclarativeType<OnGitHubWatchEvent>(OnGitHubWatchEvent.Kind);
            yield return new DeclarativeType<OnGitHubWorkflowDispatchEvent>(OnGitHubWorkflowDispatchEvent.Kind);
            yield return new DeclarativeType<OnGitHubWorkflowRunEvent>(OnGitHubWorkflowRunEvent.Kind);
        }

        /// <summary>
        /// Gets JsonConverters for DeclarativeTypes.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        /// <returns>enumeration of json converters.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}
