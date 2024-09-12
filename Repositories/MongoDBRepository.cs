using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using proyectoDivisas.Models;

namespace proyectoDivisas.Repositories
{
    public class MongoDBRepository
    {
        public MongoClient client; //coneccion entre la app y la db
        public IMongoDatabase db;
        public MongoDBRepository(IConfiguration configuration)
        {
            var mongoSettings = new MongoSettings();
            configuration.GetSection("MongoSettings").Bind(mongoSettings);

            client = new MongoClient(mongoSettings.ConnectionString);
            db = client.GetDatabase(mongoSettings.DatabaseName);
        }
    }
}
