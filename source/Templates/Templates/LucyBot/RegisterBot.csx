#r "nuget: Newtonsoft.Json, 12.0.3"
#r "nuget: MedallionShell, 1.6.1"
#r "nuget: CShell, 1.2.4"

// to install dotnet-script on your computer:
//      dotnet tool install -g dotnet-script
//
// WINDOWS: on windows you need to register .csx extensions as executable scripts:
//      dotnet script register
//
// MAC/LINUX you need to mark each script file as executable
//     chmod +x filename.csx

using CShellNet;
using System.Threading.Tasks;
using Medallion.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

new Script ().Main(Args).Wait();

class Script : CShell
{
    public async Task Main(IList<string> args)
    {
        if (args.Count != 3)
        {
            Console.WriteLine("registerbot.csx resourceGroup serviceName botId ");
            Console.WriteLine("Will create bot registration, update service config, local.settings.json and create .bot file");
            return;
        }

        string resourceGroup = args[0];
        string serviceName = args[1];
        string botId = args[2];
        Console.WriteLine($"ResourceGroup: {resourceGroup}");
        Console.WriteLine($"ServiceName: {serviceName}");
        Console.WriteLine($"BotId: {botId}");

        string endpoint = $"https://{serviceName}.azurewebsites.net/api/messages";
        string appPassword = Guid.NewGuid().ToString("n").Replace("a", "!").Replace("d","$").Replace("e",".");

        dynamic  result = await Cmd($"az ad app create --display-name {botId} --password {appPassword} --available-to-other-tenants true").AsJson();
        
        string appId = (string)result.appId;

        // create bot registration
        result = await Cmd($"az bot create --kind registration -g {resourceGroup} -n {botId} --appid {appId} --endpoint {endpoint}").AsJson();

        // save remote configuration.
        await Cmd($"az functionapp config appsettings set --name {serviceName} --resource-group {resourceGroup} --settings BotId={botId} MicrosoftAppId={appId} MicrosoftAppPassword={appPassword}").Execute();

        // save local configuration
        if (File.Exists("local.settings.json"))
        {
            result = JsonConvert.DeserializeObject(File.ReadAllText("local.settings.json"));
            result.Values["BotId"] = botId;
            result.Values["MicrosoftAppId"] = appId;
            result.Values["MicrosoftAppPassword"] = appPassword;
            File.WriteAllText("local.settings.json", JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.WriteLine("Updating local.settings.json");
            
            // await Cmd($"dotnet user-secrets set MicrosoftAppPassword \"{appPassword}\"").Execute();
        }

        result = new JObject();
        result.version = "2.0";
        result.name = botId;
        result.services = new JArray();
        
        result.services.Add(JObject.FromObject(new {
            type = "endpoint",
            appId = appId,
            appPassword = appPassword,
            endpoint = endpoint,
            id = serviceName,
            name = serviceName
        }));

        result.services.Add(JObject.FromObject(new {
            type = "endpoint",
            appId = appId,
            appPassword = appPassword,
            endpoint = "http://localhost:7071/api/messages",
            id = "localhost:7071",
            name = "localhost:7071"
        }));
       

        Console.WriteLine($"Creating {botId}.bot");
        File.WriteAllText($"{botId}.bot", JsonConvert.SerializeObject(result, Formatting.Indented));
    }
}
