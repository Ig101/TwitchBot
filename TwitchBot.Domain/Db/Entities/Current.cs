using System;
using MongoDB.Bson.Serialization.Attributes;
using TwitchBot.Domain.Enums;

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
        
        [BsonElement("t")]
        public CurrentType Type { get; set; }
    }
}