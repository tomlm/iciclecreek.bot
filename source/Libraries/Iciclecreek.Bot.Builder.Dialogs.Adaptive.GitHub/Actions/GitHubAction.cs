using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Octokit;
using System.ComponentModel.DataAnnotations;

namespace GitHubClient
{
    /// <summary>
    /// Base class for credentials of GitHubActions
    /// </summary>
    public abstract class GitHubAction : Dialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Get"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GitHubAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the property path to store the API result in.
        /// </summary>
        /// <value>
        /// The property path to store the API result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        /// <summary>
        /// Gets or sets the github productheadervalue product name to use.
        /// </summary>
        /// <value>
        /// The ProductName to use. (Default value is to use settings.github.productname)
        /// </value>
        public StringExpression ProductName { get; set; } = "=settings.github.productname";

        /// <summary>
        /// Gets or sets the GitHub private key to use to make calls with bot App credentials.
        /// </summary>
        /// <value>
        /// The private key (contents of .pem) you generated and saved previously for making calls as the Bot.
        /// (The default value is to use 'settings.github.privatekey' as the value.)
        /// </value>
        [JsonProperty("githubPrivateKey")]
        public StringExpression GitHubPrivateKey { get; set; } = "=settings.github.appPrivateKey";

        /// <summary>
        /// Gets or sets the GitHub ApplicationId to use to make calls with bot App credentials.
        /// </summary>
        /// <value>
        /// The github Application Id for making calls as the Bot.
        /// (The default value is to use 'settings.github.appId' as the value.)
        /// </value>
        [JsonProperty("githubAppId")]
        public IntExpression GitHubAppId { get; set; } = "=settings.github.appid";

        /// <summary>
        /// Gets or sets the user oauthToken to use to delegated actions for the user.
        /// </summary>
        /// <value>
        /// A token usable as OAuth credentials for calling APIs.
        /// </value>
        [JsonProperty("oauthToken")]
        public StringExpression OAuthToken { get; set; }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object opts = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (opts != null)
            {
                throw new NotSupportedException($"{nameof(opts)} is not supported by this action.");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var gitHubClient = GetGitHubClient(dc);

            var result = await CallGitHubApi(dc, gitHubClient, cancellationToken).ConfigureAwait(false);
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), result);
            }
            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method which calls the github API and returns the result.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <param name="gitHubClient">githubClient to use.</param>
        /// <param name="cancellationToken">cancealltion token.</param>
        /// <returns>result of API to use for resultProperty/EndDIalog result.</returns>
        protected abstract Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        private Octokit.GitHubClient GetGitHubClient(DialogContext dc)
        {
            var productName = this.ProductName.GetValue(dc.State);
            var oAuthToken = this.OAuthToken?.GetValue(dc.State);
            var privateKey = this.GitHubPrivateKey?.GetValue(dc.State);
            var appId = this.GitHubAppId?.GetValue(dc.State);

            if (!String.IsNullOrEmpty(oAuthToken))
            {
                return new Octokit.GitHubClient(new ProductHeaderValue(productName))
                {
                    Credentials = new Credentials(oAuthToken)
                };
            }
            else if (!String.IsNullOrEmpty(privateKey))
            {
                var generator = new GitHubJwt.GitHubJwtFactory(
                    new GitHubJwt.StringPrivateKeySource(privateKey),
                    new GitHubJwt.GitHubJwtFactoryOptions
                    {
                        AppIntegrationId = appId.Value, // The GitHub App Id
                        ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                    }
                );

                var jwtToken = generator.CreateEncodedJwtToken();

                // Pass the JWT as a Bearer token to Octokit.net
                return new Octokit.GitHubClient(new ProductHeaderValue(productName))
                {
                    Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
                };
            }

            return new Octokit.GitHubClient(new ProductHeaderValue(productName));
        }
    }
}
