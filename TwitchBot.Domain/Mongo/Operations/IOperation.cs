using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace TwitchBot.Domain.Mongo.Operations
{
    internal interface IOperation
    {
        Task ProcessAsync(IClientSessionHandle session, CancellationToken token);
    }
}