using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;

namespace SpeechAPI
{
    class SpeechTextRecognizer
    {
        private static SpeechRecognizer speechRecognizer;
        private string _speechSubscriptionKey;
        private string _speechServiceRegion;
        private delegate void SendMessageCallback(HeroCard card, string msg);
        private delegate void SendMessageUpdateCallback(HeroCard card, string msg);

        public SpeechTextRecognizer(IConfiguration config)
        {
            _speechSubscriptionKey = config["SpeechSubscriptionKey"];
            _speechServiceRegion = config["SpeechServiceRegion"];
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
            string[] languages = { "ja-JP" }; //, "en-US", "en-IN", "en-GB" };
            var language = AutoDetectSourceLanguageConfig.FromLanguages(languages);
            var stopRecognition = new TaskCompletionSource<int>();
            HeroCard card = null;
            bool createCard = true;

            SendMessageCallback msgDelegate = async (HeroCard card, string message) =>
            {
                var userName = turnContext.Activity.From.Name;
                card.Text = message;
                card.Subtitle = card.Subtitle + $"<{userName}>";
                var activity = MessageFactory.Attachment(card.ToAttachment());

                await turnContext.SendActivityAsync(activity);
            };

            SendMessageUpdateCallback updateDelegate = async (HeroCard card, string message) =>
            {
                card.Title = "I've been updated";
                card.Text = message;
                var activity = MessageFactory.Attachment(card.ToAttachment());
                if(activity.Id == null)
                {
                    activity.Id = turnContext.Activity.ReplyToId;
                }

                await turnContext.UpdateActivityAsync(activity);
            };

            using (var audioConfig = AudioConfig.FromDefaultMicrophoneInput())
            {

                using (var recognizer = new SpeechRecognizer(config, language, audioConfig))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        string message = e.Result.Text;
                        Console.WriteLine($"RECOGNIZING: Text={message}");

                        //Kim: Maybe for the processing speed of async, the update process seems to be delayed than recognizing.
                        //it should find a workaround bypassing and pending process before submitting it.
                        //if (createCard)
                        //{
                        //    card = new HeroCardCaption().createCard();
                        //    msgDelegate(card, message);
                        //    createCard = false;
                        //}
                        //else
                        //{
                        //    updateDelegate(card, message);
                        //}
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            string message = e.Result.Text;

                            card = new HeroCardCaption().createCard();
                            msgDelegate(card, message);

                            //updateDelegate(card, message);
                            //createCard = true;
                            Console.WriteLine($"RECOGNIZED: Text={message}");
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

                    speechRecognizer = recognizer;
                }
            }
        }

        public async Task RecognizeSpeechContinualAsyncStop()
        {
            if(speechRecognizer != null)
            {
                // Stops recognition.
                await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }

        }

    }
}