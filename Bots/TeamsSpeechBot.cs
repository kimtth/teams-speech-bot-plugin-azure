// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SpeechAPI;

namespace Microsoft.BotBuilderSamples.Bots
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
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("help") || text.Contains("menu"))
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
            else if (text.Contains("team"))
            {
                await TeamsFunctionActivityAsync(turnContext, cancellationToken);
            }
            else if (text.Contains("notice"))
            {
                await NotificationActivityAsync(turnContext, cancellationToken);
            }
            else
            {
                await MenuCardActivityAsync(turnContext, cancellationToken);
            }
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

        private async Task StartRecordActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await speechRecognizer.RecognizeSpeechContinualAsyncStart(turnContext);

            var message = MessageFactory.Text($"Start Recording -----");
            await turnContext.SendActivityAsync(message);
        }

        private async Task StopRecordActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if(speechRecognizer != null)
                await speechRecognizer.RecognizeSpeechContinualAsyncStop();

            var message = MessageFactory.Text($"Stop Recording -----");
            await turnContext.SendActivityAsync(message);
        }

        private async Task MenuCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Buttons = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Start Recording",
                                Text = "Start Recording"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Stop Recording",
                                Text = "Stop Recording"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Help",
                                Text = "Help"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Teams Me",
                                Text = "Team"
                            },
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Teams Notification",
                                Text = "Notice"
                            }
                        }
            };

            await SendMenuCard(turnContext, card, cancellationToken);
        }

        private async Task SendMenuCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            card.Title = "Welcome!";

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

        //private async Task GetSingleMemberAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    var member = new TeamsChannelAccount();

        //    try
        //    {
        //        member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
        //    }
        //    catch (ErrorResponseException e)
        //    {
        //        if (e.Body.Error.Code.Equals("MemberNotFoundInConversation"))
        //        {
        //            await turnContext.SendActivityAsync("Member not found.");
        //            return;
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }

        //    var message = MessageFactory.Text($"You are: {member.Name}.");
        //    var res = await turnContext.SendActivityAsync(message);

        //}

        //private async Task DeleteCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
        //}

        //// If you encounter permission-related errors when sending this message, see
        //// https://aka.ms/BotTrustServiceUrl
        //private async Task MessageAllMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
        //    var serviceUrl = turnContext.Activity.ServiceUrl;
        //    var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
        //    ConversationReference conversationReference = null;

        //    var members = await GetPagedMembers(turnContext, cancellationToken);

        //    foreach (var teamMember in members)
        //    {
        //        var proactiveMessage = MessageFactory.Text($"Hello {teamMember.GivenName} {teamMember.Surname}. I'm a Teams conversation bot.");

        //        var conversationParameters = new ConversationParameters
        //        {
        //            IsGroup = false,
        //            Bot = turnContext.Activity.Recipient,
        //            Members = new ChannelAccount[] { teamMember },
        //            TenantId = turnContext.Activity.Conversation.TenantId,
        //        };

        //        await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
        //            teamsChannelId,
        //            serviceUrl,
        //            credentials,
        //            conversationParameters,
        //            async (t1, c1) =>
        //            {
        //                conversationReference = t1.Activity.GetConversationReference();
        //                await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
        //                    _appId,
        //                    conversationReference,
        //                    async (t2, c2) =>
        //                    {
        //                        await t2.SendActivityAsync(proactiveMessage, c2);
        //                    },
        //                    cancellationToken);
        //            },
        //            cancellationToken);
        //    }

        //    await turnContext.SendActivityAsync(MessageFactory.Text("All messages have been sent."), cancellationToken);
        //}

        //private static async Task<List<TeamsChannelAccount>> GetPagedMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    var members = new List<TeamsChannelAccount>();
        //    string continuationToken = null;

        //    do
        //    {
        //        var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
        //        continuationToken = currentPage.ContinuationToken;
        //        members = members.Concat(currentPage.Members).ToList();
        //    }
        //    while (continuationToken != null);

        //    return members;
        //}

        //private static async Task SendWelcomeCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        //{
        //    var initialValue = new JObject { { "count", 0 } };
        //    card.Title = "Welcome!";
        //    card.Buttons.Add(new CardAction
        //    {
        //        Type = ActionTypes.MessageBack,
        //        Title = "Update Card",
        //        Text = "UpdateCardAction",
        //        Value = initialValue
        //    });

        //    var activity = MessageFactory.Attachment(card.ToAttachment());

        //    await turnContext.SendActivityAsync(activity, cancellationToken);
        //}

        private static async Task SendUpdatedCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, CancellationToken cancellationToken)
        {
            card.Title = "I've been updated";

            var data = turnContext.Activity.Value as JObject;
            data = JObject.FromObject(data);
            data["count"] = data["count"].Value<int>() + 1;
            card.Text = $"Update count - {data["count"].Value<int>()}";

            card.Buttons.Add(new CardAction
            {
                Type = ActionTypes.MessageBack,
                Title = "Update Card",
                Text = "UpdateCardAction",
                Value = data
            });

            var activity = MessageFactory.Attachment(card.ToAttachment());
            activity.Id = turnContext.Activity.ReplyToId;

            await turnContext.UpdateActivityAsync(activity, cancellationToken);
        }

    }
}
