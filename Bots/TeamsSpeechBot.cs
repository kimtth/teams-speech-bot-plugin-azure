// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Repository;
using Services;
using SpeechAPI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TranslateService;

namespace Microsoft.BotBuilder.Bots
{
    public class TeamsSpeechBot : TeamsActivityHandler
    {
        private string _appId;
        private string _appPassword;
        private readonly IConfiguration _config;
        private ISpeechToTextService _speechRecognizer;
        private IInfoRepository _repository;
        private ITranslateService _translator;

        public TeamsSpeechBot(IConfiguration config, IInfoRepository repository, ISpeechToTextService speechRecognizer, ITranslateService translator)
        {
            _config = config;
            _repository = repository;
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
            _translator = translator;
            _speechRecognizer = speechRecognizer;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text;
            if (turnContext.Activity.Text == null)
            {
                //Console.WriteLine(turnContext.Activity.Value);
                var jobject = turnContext.Activity.Value as JObject; //Kim: When get data from submit action of adaptive card.
                text = jobject.GetValue("command").Value<string>();

                if (text.Contains("config"))
                {
                    var setting_language = jobject.GetValue("setting_language").Value<string>();
                    _repository.SetSetting("language", setting_language);
                }
            }
            else
            {
                text = turnContext.Activity.Text.Trim().ToLower();
            }

            if (text.Contains("hi") || text.Contains("hello") || text.Contains("help") || text.Contains("menu"))
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
            else if (text.Contains("config"))
            {
                await ConfigurationActivityAsync(turnContext, cancellationToken);
            }
            else
            {
                await MenuCardActivityAsync(turnContext, cancellationToken);
            }
        }

        private async Task ConfigurationActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var language_setting = _repository.GetSetting("language");
            var language_message = language_setting.Contains("ja") ? "Japanese" : "English";
            var message = MessageFactory.Text($"Got it! 👌 The Recognition Language is set as {language_message}.");
            await turnContext.SendActivityAsync(message);
        }

        private async Task SettingActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var cardAttachment = AdaptiveCardFactory.getCard("SETTING");
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

        private async Task DeleteCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
        }

        private async Task StartRecordActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text($"Let me start recording !!");
            await turnContext.SendActivityAsync(message);

            await _speechRecognizer.RecognizeSpeechContinualAsyncStart(turnContext);
        }

        private async Task StopRecordActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (_speechRecognizer != null)
                await _speechRecognizer.RecognizeSpeechContinualAsyncStop();

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
