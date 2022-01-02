using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TwitchBot.Domain.Mongo
{
  public class MongoConnection : IMongoConnection
  {
        private readonly IMongoClient _client;
        private readonly IDictionary<Type, object> _collections = new Dictionary<Type, object>();

        public bool UseTransactions { get; private set; }

        public MongoConnection(Assembly assembly, IOptions<MongoConnectionSettings> connection,  IServiceProvider provider)
        {
            var mongoUrl = new MongoUrl(connection.Value.ConnectionString);
            var databaseName = mongoUrl.DatabaseName;
            UseTransactions = mongoUrl.RetryWrites != false;

            _client = new MongoClient(mongoUrl);

            var types = assembly
                .GetTypes()
                .Where(type => IsMongoContext(type.BaseType))
                .ToList();
            var configTypes = assembly
                .GetTypes()
                .Select(type => new
                {
                    EntityType = GetMongoConfigEntity(type),
                    Type = type
                })
                .Where(type => type.EntityType != null)
                .ToList();
            foreach (var type in types)
            {
                var entities = type
                    .GetProperties()
                    .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(IRepository<>))
                    .Select(x => new
                    {
                        Type = x.PropertyType.GetGenericArguments().First(),
                        x.Name
                    })
                    .ToList();
                var settingsType = typeof(MongoContextSettings<>).MakeGenericType(type);
                var optionsType = typeof(IOptions<>).MakeGenericType(settingsType);
                var options = provider.GetRequiredService(optionsType);
                var config = (IMongoContextSettings)optionsType.GetProperty("Value").GetValue(options);
                var database = _client.GetDatabase(databaseName ?? "default");
                foreach (var entity in entities)
                {
                    var method = database.GetType().GetMethod("GetCollection").MakeGenericMethod(entity.Type);
                    var collection = method.Invoke(database, new object[] { config.NamespaceName + "_" + entity.Name, null });
                    _collections.Add(entity.Type, collection);
                    var entityConfigs = configTypes.Where(x => x.EntityType == entity.Type).Select(x => x.Type).ToList();
                    foreach (var entityConfig in entityConfigs)
                    {
                        var instance = Activator.CreateInstance(entityConfig);
                        ((Task)entityConfig.GetMethod("ConfigureAsync").Invoke(instance, new object[] { collection })).Wait();
                    }
                }
            }
        }

        private bool IsMongoContext(Type type)
        {
            if (type == typeof(BaseMongoContext))
            {
                return true;
            }

            if (type == null)
            {
                return false;
            }

            return IsMongoContext(type.BaseType);
        }

        private Type GetMongoConfigEntity(Type type)
        {
            if (type != null)
            {
                var configuration = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityConfiguration<>));
                if (configuration != null)
                {
                    return configuration.GetGenericArguments().First();
                }
            }

            if (type == null)
            {
                return null;
            }

            return GetMongoConfigEntity(type.BaseType);
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return (IMongoCollection<T>)_collections[typeof(T)];
        }

        public IClientSessionHandle StartSession()
        {
            return _client.StartSession();
        }
    }
}