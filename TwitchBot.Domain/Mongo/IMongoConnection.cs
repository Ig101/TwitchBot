using MongoDB.Driver;

namespace TwitchBot.Domain.Mongo
{
    public interface IMongoConnection
    {
        bool UseTransactions { get; }

        IMongoCollection<T> GetCollection<T>();

        IClientSessionHandle StartSession();
    }
}