using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using TeamsCaptionBot.Resources;
using Microsoft.Bot.Builder;

public class HeroCardCaption
{

    public HeroCard createCard()
    {
        var heroCard = new HeroCard
        {
            Subtitle = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Buttons = new List<CardAction> {
                new CardAction
                {
                    Type = ActionTypes.MessageBack,
                    Title = "Delete",
                    Text = "Delete"
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