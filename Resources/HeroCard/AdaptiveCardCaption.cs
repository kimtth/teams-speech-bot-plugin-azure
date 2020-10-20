using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.IO;

namespace AdaptiveCards
{
    public class AdaptiveCardCaption : IAdaptiveCard
    {
        public Attachment createCard(string message)
        {
            var filePath = Path.Combine(".", "Resources", "MessageCard.json");
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}