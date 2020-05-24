![icon](icon.png)

# Azure Storage Table Actions
This library provides custom actions for Bot Framework/Composer to perform database operations against azure storage tables.

## Installation
To install into your project run the cli:

```shell
dotnet install Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage
```

In your startup code add:

```csharp
ComponentRegistration.Add(new AzureStorageComponentRegistration());
```

To add to your schema for usage in Bot Framework Composer from cli:

```shell
bf dialog:merge -p yourproj.proj -o your.schema
```

## Library
This library adds actions for Azure Table Stores.

### Tables
| Action                            | Description                     |
|-----------------------------------|---------------------------------|
| **Iciclecreek.Table.CreateTable** | create a table in table storage |
| **Iciclecreek.Table.DeleteTable** | delete a table in table storage |

### Entities
| Action                                | Description                                   |
|---------------------------------------|-----------------------------------------------|
| **Iciclecreek.Table.EntityOperation** | perform entity operation (Insert/Delete, etc) |
| **Iciclecreek.Table.RetrieveEntity**  | get an entity from a table by pk/rowkey       |

## Sample Json

```json
{
    "$kind": "Iciclecreek.Table.CreateTable",
    "connectionString": "=settings.connectionString",
    "table":"TestTableActions"
},
{
    "$kind": "Microsoft.SetProperty",
    "property": "$entity",
    "value": {
        "partitionKey":"xyz",
        "rowKey":"abc",
        "name":"Superman"
    }
},
{
    "$kind": "Iciclecreek.Table.EntityOperation",
    "connectionString": "=settings.connectionString",
    "table":"TestTableActions",
    "operation": "InsertOrReplace",
    "entity": "=$entity",
    "resultProperty": "$entity"
},
{
    "$kind": "Microsoft.SendActivity",
    "activity": "${$entity.etag}"
},
{
    "$kind": "Iciclecreek.Table.RetrieveEntity",
    "connectionString": "=settings.connectionString",
    "table":"TestTableActions",
    "partitionKey": "xyz",
    "rowKey": "abc",
    "resultProperty": "$entity3"
},
```
