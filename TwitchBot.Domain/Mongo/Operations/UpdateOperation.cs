using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace TwitchBot.Domain.Mongo.Operations
{
    public class UpdateOperation<T> : IOperation
    {
        private readonly IMongoCollection<T> _mongoCollection;

        private readonly Expression<Func<T, bool>> _filter;

        private readonly UpdateDefinition<T> _update;

        public UpdateOperation(IMongoCollection<T> mongoCollection, Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            _mongoCollection = mongoCollection;
            _filter = filter;
            _update = update;
        }

        public async Task ProcessAsync(IClientSessionHandle session, CancellationToken token)
        {
            if (session != null)
            {
                await _mongoCollection.UpdateManyAsync(session, _filter, _update, null, token);
            }
            else
            {
                await _mongoCollection.UpdateManyAsync(_filter, _update, null, token);
            }
        }
    }
}