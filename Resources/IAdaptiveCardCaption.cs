using Microsoft.Bot.Schema;

namespace TeamsCaptionBot.Resources
{
    interface IAdaptiveCardCaption
    {
        Attachment createCard(string message);
    }
}