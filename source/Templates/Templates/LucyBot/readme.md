# Bot
This bot project is an Azure Function project which is set up with LucyRecognizer.

## To run locally

### From VS
F5 to build and run.

### from CLI
```func start```

## To publish bot to Azure
1. Right click Publish to Azure 
1.  copy off endpoint as **cloudendpoint** (https://mybot.azurewebsites.net/api/messages)

## To register Bot
1. Create **registration only bot**, set endpoint to the **cloudendpoint** (from publish step) and save off **BotId**
1. Copy the generated appId, save off as **MicrosoftAppId**
1. Click on application 'Manage application', create new password, save the value as **MicrosoftAppPassword**

## To configure Azure Bot
 Go to Azure Function portal and edit bot's configuration
1. set **BotId** => *value you saved for BotId*
1. set **MicrosoftAppId** => *value you saved for MicrosoftAppId*
1. set **MicrosoftAppPassword** => *value you saved for MicrosoftAppPassword**
> SAVE CHANGES <-- *portal will just silently drop your changes*

## To configure local csproj
Go to local csproj and edit local.settings.json
1. set **BotId** => *value you saved for BotId*
1. set **MicrosoftAppId** => *value you saved for MicrosoftAppId*
1. set **MicrosoftAppPassword** => *value you saved for MicrosoftAppPassword**

## To set up emulator
Open emulator and create new bot
1. Add localhost endpoint 
    * set **endpoint** ==> http://localhost:7071/api/messages
    * set **BotId** => *value you saved for BotId*
    * set **MicrosoftAppId** => *value you saved for MicrosoftAppId*
    * set **MicrosoftAppPassword** => *value you saved for MicrosoftAppPassword*
2. Add cloud endpoint 
    * set **endpoint** ==> **cloudendpoint**
    * set **BotId** => *value you saved for BotId*
    * set **MicrosoftAppId** => *value you saved for MicrosoftAppId*
    * set **MicrosoftAppPassword** => *value you saved for MicrosoftAppPassword*

3. Save bot file.

## To connect to the local bot
Open remote endpoint and talk to bot.

## To connect to the cloud bot
Open remote endpoint and talk to bot.

## To debug using VS Code Bot Debugger
* Use VSCode to **Attach Bot Debugger** on port 5001


