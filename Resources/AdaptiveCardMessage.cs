using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AdaptiveCards
{
    public class AdaptiveCardMessage : IAdaptiveCard
    {
        public Attachment createCard(string message)
        {
            var filePath = Path.Combine(".", "Resources", "MessageCard", "payload.json");
            var adaptiveCardJson = File.ReadAllText(filePath);
            adaptiveCardJson = messageUpdate(adaptiveCardJson, message);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        // Kim: https://docs.microsoft.com/en-us/adaptive-cards/templating/sdk
        private string messageUpdate(string adaptiveCardJson, string message)
        {
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);

            var myData = new
            {
                Src_language = message,
                Target_language = "Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation.",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            string cardJson = template.Expand(myData);

            return cardJson;
        }
    }
}