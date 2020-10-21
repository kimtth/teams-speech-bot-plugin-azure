﻿
# Teams Conversation Bot 
## Integration with The Speech service, part of Azure Cognitive Services

Bot Framework v4 Conversation Bot for Teams.

This bot has been created using [Bot Framework](https://dev.botframework.com). This sample shows
how to incorporate basic conversational flow into a Teams application. It also illustrates a few of the Teams specific calls you can make from your bot.

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## To try this sample

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/{userId}/teams-minutes-bot-plugin-azure.git
    ```

1) Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 -host-header="localhost:3978"
    ```

1) Create [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running ngrok. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Bot Framework registration](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/create-a-bot-for-teams#register-your-web-service-with-the-bot-framework)

1) Update the `appsettings.json` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

1) Run your bot, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

## Interacting with the bot

You can interact with this bot by sending it a message, or selecting a command from the command list. The bot will respond to the following strings.

1. **Start Recording**
  - **Result:** The bot will send a request to Speech SDK for starting voice recognition.
  - **During Recognizing**
  ![Recognizing](./Screenshot/recognizing.png "Recognizing Dialog")
  - **Recognized:** The bot will send a response of speech recognition result with translated text, which is translated by Azure Translate, part of Azure Cognitive Service. 
  ![Recognized](./Screenshot/message.png "Recognized Dialog")
2. **Stop Recording**
  - **Result:** The bot will send a request to Speech SDK for stopping voice recognition.
3. **Me**
  - **Result:** The bot will send a response of who is a current user that is fetched from Teams Context.
4. **Help**
- **Result:** The bot will send the welcome card with supported commands list.
![Welcome](./Screenshot/welcome.png "Welcome Dialog")
5. **Settings**
- **Result:** The configuration dialog for setting source language of Speech recognition.
![Settings](./Screenshot/setting.png "Settings Dialog")

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [How Microsoft Teams bots work](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics-teams?view=azure-bot-service-4.0&tabs=javascript)

