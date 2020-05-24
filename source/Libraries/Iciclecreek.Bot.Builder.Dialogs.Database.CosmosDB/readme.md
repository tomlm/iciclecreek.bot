![icon](icon.png)

# Comos Actions
This library provides custom actions for Bot Framework/Composer to do operations against cosmos stores.

## Installation
To install into your project run the cli:

```dotnet install Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos```

In your startup code add:

```ComponentRegistration.Add(new CosmosComponentRegistration());```

To add to your schema for usage in Bot Framework Composer from cli:

```bf dialog:merge -p yourproj.proj -o your.schema```

## Library
This library adds actions for comos datastores.

### Database
| Action                                | Description       |
|---------------------------------------|-------------------|
| **Iciclecreek.Cosmos.CreateDatabase** | create a database |
| **Iciclecreek.Cosmos.DeleteDatabase** | delete a database |

### Containers
| Action                                 | Description        |
|----------------------------------------|--------------------|
| **Iciclecreek.Cosmos.CreateContainer** | create a Container |
| **Iciclecreek.Cosmos.DeleteContainer** | delete a Container |

### Items
| Action                             | Description                      |
|------------------------------------|----------------------------------|
| **Iciclecreek.Cosmos.CreateItem**  | create an item in a Container    |
| **Iciclecreek.Cosmos.GetItem**     | get an item from a Container     |
| **Iciclecreek.Cosmos.ReplaceItem** | replace an item from a Container |
| **Iciclecreek.Cosmos.DeleteItem**  | delete an item in a Container    |
| **Iciclecreek.Cosmos.QueryItems**  | query for items in a Container   |

## Sample Json

```json
{
    "$kind": "Iciclecreek.Cosmos.CreateDatabase",
    "connectionString": "=settings.cosmosConnectionString",
    "database": "mydb"
},
{
    "$kind": "Iciclecreek.Cosmos.CreateContainer",
    "connectionString": "=settings.cosmosConnectionString",
    "database": "mydb",
    "container": "mycont",
    "partitionKey": "/id"
},
{
    "$kind": "Microsoft.SetProperties",
    "assignments": [
        {
            "property": "$entity.id",
            "value": "123123"
        },
        {
            "property": "$entity.name",
            "value": "Superman"
        }
    ]
},
{
    "$kind": "Iciclecreek.Cosmos.CreateItem",
    "connectionString": "=settings.cosmosConnectionString",
    "database": "mydb",
    "container": "mycont",
    "item": "=$entity",
    "resultProperty": "$entity"
},
```
