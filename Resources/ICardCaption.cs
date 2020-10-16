using Microsoft.Bot.Schema;

namespace TeamsCaptionBot.Resources
{
    interface ICardCaption
    {
        Attachment createCard(string message);
    }
}