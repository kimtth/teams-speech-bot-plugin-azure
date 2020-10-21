using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Repository;
using System;
using System.Threading.Tasks;
using TranslateService;

namespace SpeechAPI
{
    class SpeechTextRecognizer
    {
        private static SpeechRecognizer speechRecognizer;
        private string _speechSubscriptionKey;
        private string _speechServiceRegion;
        private Translator _translator;
        private InMemoryRepository _repository;
        private delegate void SendMessageCallback(Attachment card, bool isInitialRecognizing);
        private delegate void SendMessageDeleteCallback(string activityId);

        public SpeechTextRecognizer(IConfiguration config, Translator translator, InMemoryRepository repository)
        {
            _speechSubscriptionKey = config["SpeechSubscriptionKey"];
            _speechServiceRegion = config["SpeechServiceRegion"];
            _repository = repository;
            _translator = translator;
        }

        public async Task RecognizeSpeechOnceAsync()
        {
            var config = SpeechConfig.FromSubscription(_speechSubscriptionKey, _speechServiceRegion);

            using (var recognizer = new SpeechRecognizer(config))
            {
                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"We recognized: {result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

        public async Task RecognizeSpeechContinualAsyncStart(ITurnContext<IMessageActivity> turnContext)
        {
            var config = SpeechConfig.FromSubscription(_speechSubscriptionKey, _speechServiceRegion);
            var language_setting = _repository.GetSetting("language");
            language_setting = string.IsNullOrEmpty(language_setting) ? "ja-JP" : language_setting;

            string[] languages = { language_setting };
            var language = AutoDetectSourceLanguageConfig.FromLanguages(languages);
            var stopRecognition = new TaskCompletionSource<int>();
            string tempCardActivityId = "";
            bool isInitialRecognizing = true;

            SendMessageCallback msgDelegate = async (Attachment card, bool isInitialRecognizing) =>
            {
                var activity = MessageFactory.Attachment(card);
                ResourceResponse response = await turnContext.SendActivityAsync(activity);

                if(isInitialRecognizing)
                    tempCardActivityId = response.Id;
                
                Console.WriteLine("tempCardActivityId: ", tempCardActivityId);
            };

            SendMessageDeleteCallback msgDelete = async (string activityId) =>
            {
                await turnContext.DeleteActivityAsync(activityId);
            };

            using (var audioConfig = AudioConfig.FromDefaultMicrophoneInput())
            {

                using (var recognizer = new SpeechRecognizer(config, language, audioConfig))
                {
                    speechRecognizer = recognizer;
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        string message = e.Result.Text;
                        Console.WriteLine($"RECOGNIZING: Text={message}");

                        if (isInitialRecognizing)
                        {
                            var recognizing = new AdaptiveCardRecognizing();
                            var card = recognizing.createCard();

                            msgDelegate(card, isInitialRecognizing);
                            isInitialRecognizing = false;
                        }
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            string message = e.Result.Text;
                            if(string.IsNullOrEmpty(message.Trim()) == false)
                            {
                                var caption = new AdaptiveCardMessage(_translator, _repository);
                                var card = caption.createCard(message);

                                msgDelegate(card, isInitialRecognizing);

                                isInitialRecognizing = true;

                                if (string.IsNullOrEmpty(tempCardActivityId) == false)
                                {
                                    msgDelete(tempCardActivityId);
                                    tempCardActivityId = "";
                                }
                                Console.WriteLine($"RECOGNIZED: Text={message}");
                            }
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });
                }
            }
        }

        public async Task RecognizeSpeechContinualAsyncStop()
        {
            try
            {
                await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }
            catch
            {
                Console.WriteLine("----", speechRecognizer);
            }
        }

    }
}