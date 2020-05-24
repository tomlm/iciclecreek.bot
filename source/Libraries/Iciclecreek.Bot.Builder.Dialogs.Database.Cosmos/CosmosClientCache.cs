using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos
{
    internal static class CosmosClientCache
    {
        private static Dictionary<string, CosmosClient> clients = new Dictionary<string, CosmosClient>();

        public static CosmosClient GetClient(string connectionString)
        {
            lock (clients)
            {
                if (clients.TryGetValue(connectionString, out var client))

                {
                    return client;
                }

                client = new CosmosClient(connectionString);
                clients[connectionString] = client;
                return client;
            }
        }
    }
}
