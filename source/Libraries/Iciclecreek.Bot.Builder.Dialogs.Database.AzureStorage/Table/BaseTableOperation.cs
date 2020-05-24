using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage.Table
{
    /// <summary>
    /// Execute SQL against SqlClient.
    /// </summary>
    public abstract class BaseTableOperation : Dialog
    {
        /// <summary>
        /// Gets or sets the ConnectionString for querying the database.
        /// </summary>
        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [JsonProperty("table")]
        public StringExpression Table { get; set; }

        protected CloudTable GetCloudTable(DialogContext dc)
        {
            var connectionString = ConnectionString.GetValue(dc.State);
            var tableName = Table.GetValue(dc.State);

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        protected DynamicTableEntity JObjectToEntity(JObject obj)
		{
			var entity = new DynamicTableEntity();
			foreach (var property in obj.Properties())
			{
				if (property.Name.ToLower() == "partitionkey")
				{
					entity.PartitionKey = property.Value.ToString();
				}
				else if (property.Name.ToLower() == "rowkey")
				{
					entity.RowKey = property.Value.ToString();
				}
				else if (property.Name.ToLower() == "etag")
				{
					entity.ETag = property.Value.ToString();
				}
				else if (property.Name.ToLower() == "timestamp")
				{
					entity.Timestamp = (DateTime)property.Value;
				}
				else
				{
					switch (property.Value.Type)
					{
						case JTokenType.Boolean:
							entity.Properties[property.Name] = new EntityProperty((bool)property.Value);
							break;
						case JTokenType.Date:
							entity.Properties[property.Name] = new EntityProperty((DateTime)property.Value);
							break;
						case JTokenType.Integer:
							entity.Properties[property.Name] = new EntityProperty((int)property.Value);
							break;
						case JTokenType.Float:
							entity.Properties[property.Name] = new EntityProperty((float)property.Value);
							break;
						case JTokenType.String:
							if (Guid.TryParse((string)property.Value, out Guid guid))
							{
								entity.Properties[property.Name] = new EntityProperty(guid);
							}
							else
							{
								entity.Properties[property.Name] = new EntityProperty((string)property.Value);
							}
							break;
					}
				}
			}
			return entity;
		}

		protected JObject EntityToJObject(DynamicTableEntity entity)
		{
			dynamic obj = new JObject();
			obj[nameof(entity.PartitionKey)] = entity.PartitionKey;
			obj[nameof(entity.RowKey)] = entity.RowKey;
			obj[nameof(entity.ETag)] = entity.ETag;
			obj[nameof(entity.Timestamp)] = entity.Timestamp;

			foreach (var kv in entity.Properties)
			{
				switch (kv.Value.PropertyType)
				{
					case EdmType.Boolean:
						obj[kv.Key] = kv.Value.BooleanValue;
						break;
					case EdmType.DateTime:
						obj[kv.Key] = kv.Value.DateTime;
						break;
					case EdmType.Int32:
						obj[kv.Key] = kv.Value.Int32Value;
						break;
					case EdmType.Int64:
						obj[kv.Key] = kv.Value.Int64Value;
						break;
					case EdmType.Double:
						obj[kv.Key] = kv.Value.DoubleValue;
						break;
					case EdmType.String:
						obj[kv.Key] = kv.Value.StringValue;
						break;
					case EdmType.Guid:
						obj[kv.Key] = kv.Value.GuidValue.ToString();
						break;
				}
			}
			return (JObject)obj;
		}
	}
}
