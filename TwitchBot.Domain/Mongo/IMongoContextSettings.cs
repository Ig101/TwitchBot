namespace TwitchBot.Domain.Mongo
{
    public interface IMongoContextSettings
    {
        string NamespaceName { get; }
    }
}