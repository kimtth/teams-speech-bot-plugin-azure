using Microsoft.Bot.Schema;

namespace AdaptiveCards
{
    interface IAdaptiveCard
    {
        Attachment createCard(string message);
    }
}