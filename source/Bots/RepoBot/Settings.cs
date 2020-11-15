// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Azure;

namespace RepoBot
{
    public partial class Settings
    {
        public string AzureWebJobsStorage { get; set; }

        public string BotId { get; set; }

        public string MicrosoftAppId { get; set; }

        public string MicrosoftAppPassword { get; set; }
    }
}
