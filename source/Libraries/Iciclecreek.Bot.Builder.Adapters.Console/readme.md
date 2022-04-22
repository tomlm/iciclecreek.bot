![icon](icon.png)

# Console Adapter
This library provides an adapter that allows you to use bot framework dialog system for console applications.

## Installation
To install into your project run the cli:

```dotnet add package Iciclecreek.Bot.Builder.Adapters.ConsoleCosmos```

In your console app:

```C#
            //setup our DI
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "defaultRootDialog", rootDialog },
                    { "applicationRoot", folder}
                })
                .AddCommandLine(args)
                .Build();

            IServiceCollection services = new ServiceCollection();
            // use file storage for userstate, memory for conversationstate
            services.AddSingleton((sp) => new UserState(new FileStorage(userStoragePath)));
            services.AddBotRuntime(configuration);
            var sp = services.BuildServiceProvider();

            // run bot on console adapter
            await new ConsoleAdapter()
                .StartConversation((tc, ct) => sp.GetService<IBot>().OnTurnAsync(tc, ct), text);
```
