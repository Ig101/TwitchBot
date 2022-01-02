using TwitchBot.Domain.Db.Entities;
using TwitchBot.Domain.Mongo;

namespace TwitchBot.Domain.Db
{
    public class DbContext : BaseMongoContext
    {
        public IRepository<Current> Currents { get; set; }

        public DbContext(IMongoConnection connection)
            : base(connection)
        {
            Currents = InitializeRepository<Current>();
        }
    }
}