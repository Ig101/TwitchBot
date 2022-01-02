using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace TwitchBot.Domain.Mongo.Operations
{
    public class DeleteFilterOperation<T> : IOperation
    {
        private readonly IMongoCollection<T> _mongoCollection;

        private readonly FilterDefinition<T> _filter;

        public DeleteFilterOperation(IMongoCollection<T> mongoCollection, FilterDefinition<T> filter)
        {
            _mongoCollection = mongoCollection;
            _filter = filter;
        }

        public async Task ProcessAsync(IClientSessionHandle session, CancellationToken token)
        {
            if (session != null)
            {
                await _mongoCollection.DeleteManyAsync(session, _filter, null, token);
            }
            else
            {
                await _mongoCollection.DeleteManyAsync(_filter, null, token);
            }
        }
    }
}