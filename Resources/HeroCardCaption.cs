using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using TeamsCaptionBot.Resources;

public class HeroCardCaption
{

    public HeroCard createCard()
    {
        var heroCard = new HeroCard
        {
            //Title = "Recognized",
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

    public Attachment updateCard(HeroCard card, string message)
    {
        card.Text = message;

        return card.ToAttachment();
    }
}