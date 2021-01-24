# Bot
This bot project is an Azure Function project which is set up with LucyRecognizer.

## To run locally

### From VS
F5 to build and run.

### from CLI
```func start```

## To publish bot to Azure
1. Right click Publish to Azure 
1.  copy off endpoint as **cloudendpoint** (https://serviceName.azurewebsites.net/api/messages)

## To register bot
```
RegisterBot.csx {resourceGroup} {serviceName} {botId}
```

This will:
* Create Bot registration for https://servicename.azurewebsites.net/api/messages
* Set **BotId**, **MicrosoftAppId**, **MicrosoftPassword** settings for serviceName
* Set **BotId**, **MicrosoftAppId**, **MicrosoftPassword** settings for local.settings.json 
* Create **{BotId}.bot** file which is configured to talk to remote and local endpoints.

