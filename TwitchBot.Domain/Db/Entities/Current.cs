using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TwitchBot.Domain.Db.Entities
{
    public class Current
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonElement("u")]
        public string UserName { get; set; }

        [BsonElement("s")]
        public double Value { get; set; }
    }
}