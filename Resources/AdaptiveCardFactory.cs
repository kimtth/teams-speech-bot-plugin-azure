using Microsoft.Bot.Schema;
using Repository;
using Services;

namespace AdaptiveCards
{
    public static class AdaptiveCardFactory
    {
        public static Attachment getCard(string shapeType)
        {
            return getCard(shapeType, "");
        }

        public static Attachment getCard(ITranslateService translator, IInfoRepository repository, string message)
        {
            return new AdaptiveCardMessage(translator, repository).createCard(message);
        }

        public static Attachment getCard(string shapeType, string message)
        {
            if (shapeType == null)
            {
                return null;
            }
            else if (shapeType.Trim().Equals("SETTING"))
            {
                return new AdaptiveCardSetting().createCard(message);

            }
            else if (shapeType.Trim().Equals("RECOGNIZING"))
            {
                return new AdaptiveCardRecognizing().createCard(message);
            }

            return null;
        }
    }
}