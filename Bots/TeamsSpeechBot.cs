// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SpeechAPI;
using AdaptiveCards;

namespace Microsoft.BotBuilder.Bots
{
    public class TeamsSpeechBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private readonly IConfiguration _config;
        private SpeechTextRecognizer speechRecognizer;

        public TeamsSpeechBot(IConfiguration config)
        {
            _config = config;
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            speechRecognizer = new SpeechTextRecognizer(_config);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text;
            if (turnContext.Activity.Text == null)
            {
                var jobject = turnContext.Activity.Value as JObject; //Kim: When get data from submit action of adaptive card.
                text = jobject.GetValue("command").Value<string>();
            }
            else
            {
                text = turnContext.Activity.Text.Trim().ToLower();
            }


            if (text.Contains("hello") || text.Contains("help") || text.Contains("menu"))
            {
                await MenuCardActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("start"))
            {
                await StartRecordActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("stop"))
            {
                await StopRecordActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("who"))
            {
                await TeamsFunctionActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("notice"))
            {
                await NotificationActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("delete"))
            {
                await DeleteCardActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("setting"))
            {
                await SettingActivityAsync(turnContext, cancellationToken);
            }
            else
            {
                await MenuCardActivityAsync(turnContext, cancellationToken);
            }
        }

        private async Task SettingActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var cardAttachment = new AdaptiveCardSetting().createCard();
            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
        }

        protected async Task NotificationActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text("You got a notification.");
            message.TeamsNotifyUser();

            await turnContext.SendActivityAsync(message);
        }

        private async Task TeamsFunctionActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var member = new TeamsChannelAccount();

            try
            {
                member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            }
            catch (ErrorResponseException e)
            {
                if (e.Body.Error.Code.Equals("MemberNotFoundInConversation"))
                {
                    await turnContext.SendActivityAsync("Member not found.");
                    return;
                }
                else
                {
                    throw e;
                }
            }

            var message = MessageFactory.Text($"You are: {member.Name}.");
            await turnContext.SendActivityAsync(message);
        }

        private async Task AdaptiveCardDisplayActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var cardAttachment = new AdaptiveCardCaption().createCard("");
            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
        }

        private async Task DeleteCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
        }

        private async Task StartRecordActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text($"Let me start recording !!");
            await turnContext.SendActivityAsync(message);

            await speechRecognizer.RecognizeSpeechContinualAsyncStart(turnContext);
        }

        private async Task StopRecordActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (speechRecognizer != null)
                await speechRecognizer.RecognizeSpeechContinualAsyncStop();

            var message = MessageFactory.Text($"Stop recording !!");
            await turnContext.SendActivityAsync(message);
        }

        private async Task MenuCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            var teamsId = turnContext.Activity.TeamsGetTeamInfo();

            var card = new HeroCardWelcome().createCard();
            await SendMenuCard(turnContext, card, cancellationToken);
        }

        private async Task SendMenuCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            var activity = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to access {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
            }
        }

    }
}
