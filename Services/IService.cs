using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace Services
{
    public interface ISpeechToTextService
    {
        Task RecognizeSpeechContinualAsyncStart(ITurnContext<IMessageActivity> turnContext);
        Task RecognizeSpeechContinualAsyncStop();
    }

    public interface ITranslateService
    {
        Task<string> TranslateExecuteAsync(string from, string to, string textToTranslate);
    }
}
//https://stackoverflow.com/questions/59370644/programmatically-sending-a-message-to-a-bot-in-microsoft-teams