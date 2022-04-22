![icon](icon.png)

# SQL Actions
This library provides custom actions for Bot Framework/Composer to do operations against SQL Database stores.

## Installation
To install into your project run the cli:

```dotnet add package Iciclecreek.Bot.Builder.Dialogs.Database.SQL```

## Library
This library adds actions for SQL Databases.

### Database
| Action                                | Description       |
|---------------------------------------|-------------------|
| **Iciclecreek.Sql.ExecuteSQL** | dexecute sql statement|


## Example
```json
  {
    "$kind": "Iciclecreek.Sql.Execute",
    "connectionString": "=settings.connectionString",
    "statements": [
        "CREATE TABLE COMPANY(",
        "   ID INT PRIMARY KEY     NOT NULL,",
        "   NAME           TEXT    NOT NULL",
        ");"
    ]
},
{
    "$kind": "Iciclecreek.Sql.Execute",
    "connectionString": "=settings.connectionString",
    "statements": [
        "INSERT INTO COMPANY (ID,NAME) VALUES (1, '${turn.name}');"
    ]
},
{
    "$kind": "Iciclecreek.Sql.Execute",
    "connectionString": "=settings.connectionString",
    "statements": [
        "SELECT NAME FROM COMPANY;"
    ],
    "resultProperty": "turn.results"
},
```
