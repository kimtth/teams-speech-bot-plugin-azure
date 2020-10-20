using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace AdaptiveCards
{
    public class HeroCardWelcome
    {

        public HeroCard createCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Welcome to AI-Caption! How can I help you?",
                Text = "Please select your language in Setting. " +
                "Once press [Start Recording] and I will detect your message and instantly translate it. " +
                "Currently, I support from English to Japanese, vice versa.",
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
                            //new CardAction
                            //{
                            //    Type = ActionTypes.MessageBack,
                            //    Title = "Me",
                            //    Text = "Who"
                            //},
                            //new CardAction
                            //{
                            //    Type = ActionTypes.MessageBack,
                            //    Title = "Notification",
                            //    Text = "Notice"
                            //},
                            new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Title = "Settings",
                                Text = "setting"
                            }
                        }
            };

            return heroCard;
        }

        public Attachment updateCard(ITurnContext<IMessageActivity> turnContext, HeroCard card, string message)
        {
            var userName = turnContext.Activity.From.Name;
            card.Text = message;
            card.Subtitle = card.Subtitle + $"<{userName}>";

            return card.ToAttachment();
        }
    }
}