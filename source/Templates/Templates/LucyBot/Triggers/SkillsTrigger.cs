// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Rest.Serialization;
using System.Collections.Generic;

namespace LucyBot
{
    public class SkillsTrigger
    {
        internal static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() }
        };

        private readonly SkillHandler _skillHandler;

        public SkillsTrigger(SkillHandler skillHandler)
        {
            this._skillHandler = skillHandler ?? throw new ArgumentNullException(nameof(skillHandler));
        }

        [FunctionName("ReplyToActivityAsync")]
        public async Task<IActionResult> ReplyToActivityAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v3/conversations/{conversationId}/activities/{activityId}")] HttpRequest req,
            string conversationId, string activityId, ILogger log)
        {
            log.LogInformation($"Skill ReplyToActivityAsync endpoint triggered.");

            var body = await req.ReadAsStringAsync();
            var activity = JsonConvert.DeserializeObject<Activity>(body, serializationSettings);
            var result = await _skillHandler.HandleReplyToActivityAsync(req.Headers["Authorization"], conversationId, activityId, activity);

            return new JsonResult(result, serializationSettings);
        }


        [FunctionName("SendToConversationAsync")]
        public async Task<IActionResult> SendToConversationAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v3/conversations/{conversationId}/activities")] HttpRequest req,
           string conversationId, ILogger log)
        {
           log.LogInformation($"Skill SendToConversationAsync endpoint triggered.");

           var body = await req.ReadAsStringAsync();
           var activity = JsonConvert.DeserializeObject<Activity>(body, serializationSettings);
           var result = await _skillHandler.HandleSendToConversationAsync(req.Headers["Authorization"], conversationId, activity);

           return new JsonResult(result, serializationSettings);
        }
    }
}
