using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Repository;
using System;
using System.IO;
using TranslateService;

namespace AdaptiveCards
{
    public class AdaptiveCardMessage : IAdaptiveCard
    {
        private Translator _translator;
        private InMemoryRepository _repository;
        public AdaptiveCardMessage(Translator translator, InMemoryRepository repository)
        {
            _translator = translator;
            _repository = repository;
        }
        public Attachment createCard(string message)
        {
            var filePath = Path.Combine(".", "Resources", "MessageCard", "payload.json");
            var adaptiveCardJson = File.ReadAllText(filePath);
            adaptiveCardJson = messageUpdateAsync(adaptiveCardJson, message);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        // Kim: https://docs.microsoft.com/en-us/adaptive-cards/templating/sdk
        private string messageUpdateAsync(string adaptiveCardJson, string message)
        {
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var from = _repository.GetSetting("language");
            from = string.IsNullOrEmpty(from) ? "ja-JP" : from;
            from = from.Contains("en") ? "en" : "ja";
            var to = from.Contains("en") ? "ja" : "en";

            System.Threading.Tasks.Task<string> task = _translator.TranslateExecuteAsync(from, to, message);
            task.Wait(); //Kim: Blocks thread and waits until task is completed
            string resultText = task.Result;

            var myData = new
            {
                Src_language = message,
                Target_language = resultText,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            string cardJson = template.Expand(myData);

            return cardJson;
        }
    }
}