using System.Threading.Tasks;
using MongoDB.Driver;

namespace TwitchBot.Domain.Mongo
{
    public interface IEntityConfiguration<Ttype>
    {
        Task ConfigureAsync(IMongoCollection<Ttype> collection);
    }
}