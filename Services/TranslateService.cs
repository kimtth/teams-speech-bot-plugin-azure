using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Services;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TranslateService
{
    /// <summary>
    /// The C# classes that represents the JSON returned by the Translator Text API.
    /// </summary>
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }

    public class Translator : ITranslateService
    {
        private string _subscriptionKey;
        private string _endpoint;
        private string _region;
        public Translator(IConfiguration config)
        {
            _subscriptionKey = config["TranslateSubscriptionKey"];
            _endpoint = config["TranslateEndPoint"];
            _region = config["TranslateServiceRegion"];
        }

        // Async call to the Translator Text API
        private async Task<string> TranslateTextRequest(string fromTo, string textToTranslate)
        {
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(_endpoint + fromTo);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);

                string resultText = "";
                foreach (TranslationResult o in deserializedOutput)
                {
                    foreach (Translation t in o.Translations)
                    {
                        Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                        resultText += t.Text; 
                    }
                }
                return resultText;
            }
        }

        // https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
        public async Task<string> TranslateExecuteAsync(string from, string to, string textToTranslate)
        {
            string fromTo = $"/translate?api-version=3.0&from={from}&to={to}";
            string resultText = await TranslateTextRequest(fromTo, textToTranslate);
            return resultText;
        }
        
    }
}
