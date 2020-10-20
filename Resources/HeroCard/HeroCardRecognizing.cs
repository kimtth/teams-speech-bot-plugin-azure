using Microsoft.Bot.Schema;
using System;
using System.IO;
using System.Collections.Generic;

namespace AdaptiveCards
{
    public class HeroCardRecognizing
    {

        public HeroCard createCard()
        {
            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources", @"Images", "loading-red-spot.gif");

            var heroCard = new HeroCard
            {
                Title = "Recognizing...",
                Subtitle = "Voice recognition has been starting.",
                Images = new List<CardImage> { new CardImage(imagePath) },
            };

            return heroCard;
        }
    }
}