using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.IO;
using TeamsCaptionBot.Resources;

public class AdaptiveCardCaption : IAdaptiveCardCaption
{
    // This array contains the file location of our adaptive cards
    private readonly string[] _cards =
    {
        Path.Combine(".", "Resources", "MessageCard.json"),
    };

    public Attachment createCard(string message)
    {
        var filePath = _cards[0];
        var adaptiveCardJson = File.ReadAllText(filePath);
        var adaptiveCardAttachment = new Attachment()
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonConvert.DeserializeObject(adaptiveCardJson),
        };
        return adaptiveCardAttachment;
    }
}