using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using TwitchBot.Domain.Mongo.Operations;

namespace TwitchBot.Domain.Mongo
{
    public abstract class BaseMongoContext
    {
        private readonly IMongoConnection _connection;
        private readonly Queue<IOperation> _operations;

        public BaseMongoContext(IMongoConnection connection)
        {
            _connection = connection;
            _operations = new Queue<IOperation>();
        }

        public async Task ApplyChangesAsync(CancellationToken token = default)
        {
            if (_operations.Count > 0)
            {
                IClientSessionHandle session = null;
                if (_connection.UseTransactions)
                {
                    session = _connection.StartSession();
                    session.StartTransaction();
                }

                try
                {
                    while (_operations.Count > 0)
                    {
                        var item = _operations.Dequeue();
                        await item.ProcessAsync(session, token);
                    }

                    if (session != null)
                    {
                        await session.CommitTransactionAsync(token);
                        session.Dispose();
                    }

                    _operations.Clear();
                }
                catch
                {
                    if (session != null)
                    {
                        await session.AbortTransactionAsync(token);
                        session.Dispose();
                    }

                    throw;
                }
            }
        }

        protected IRepository<T> InitializeRepository<T>()
        {
            var result = new Repository<T>(_connection, _operations);
            return result;
        }
    }
}